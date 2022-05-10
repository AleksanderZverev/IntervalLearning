import { yupResolver } from '@hookform/resolvers/yup';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    MenuItem,
    Select,
    Typography,
    FormControlLabel,
    Checkbox,
} from '@mui/material';
import { FC, useState } from 'react';
import { FormProvider, SubmitHandler, useFieldArray, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { useCreateScheduleMutation } from '../../redux/schedulesSlice';
import { CreateScheduleItem, ForgottenBehavior, PhaseInfo } from '../../types/schedule';
import { ForgottenBehaviorSelect } from '../ForgottenBehaviorSelect/ForgottenBehaviorSelect';
import { Form, FormField, FormFiledLabel } from '../Form/Form';

enum DurationType {
    Seconds = 1,
    Minutes = 2,
    Hours = 3,
    Days = 4,
}

function DurationTypeToSeconds(value: number, type: DurationType) {
    let result = value;

    if (type === DurationType.Days) {
        result *= 24;
        type--;
    }

    if (type === DurationType.Hours) {
        result *= 60;
        type--;
    }

    if (type === DurationType.Minutes) {
        result *= 60;
        type--;
    }

    return result;
}

interface IPhaseForm {
    durationFromLastPhase: number;
    durationType: DurationType;
    shortDescription: string | null;
    description: string | null;
    isDefaultValueSide: boolean;
    addRepeatingAtThatDay: boolean;
}

const phaseSchema = yup.object({
    durationFromLastPhase: yup.number().min(1).required(),
    durationType: yup.number().required().default(DurationType.Days),
    shortDescription: yup.string().max(100),
    description: yup.string().max(1000),
    isDefaultValueSide: yup.boolean().default(false),
    addRepeatingAtThatDay: yup.boolean().default(true),
});

interface IForm {
    cardsCountPerPhase: number;
    forgottenBehavior: ForgottenBehavior;
    title: string;
    shortDescription: string | null;
    description: string | null;
    defaultPhaseShortDescription: string | null;
    defaultPhaseDescription: string | null;
    defaultRepeatPhaseShortDescription: string | null;
    defaultRepeatPhaseDescription: string | null;
    phases: IPhaseForm[];
}

const schema = yup
    .object({
        cardsCountPerPhase: yup.number().min(0).max(9999).required(),
        title: yup.string().min(1).max(255).required(),
        forgottenBehavior: yup.number().required().default(1),
        phases: yup.array().of(phaseSchema).required(),
        shortDescription: yup.string().max(100),
        description: yup.string().max(1000),
        defaultPhaseShortDescription: yup.string().max(100),
        defaultPhaseDescription: yup.string().max(1000),
        defaultRepeatPhaseShortDescription: yup.string().max(100),
        defaultRepeatPhaseDescription: yup.string().max(1000),
    })
    .required();

interface CreateScheduleModalProps {
    open: boolean;
    onClose: () => void;
}

export const CreateScheduleModal: FC<CreateScheduleModalProps> = (props) => {
    const formMethods = useForm<IForm>({ resolver: yupResolver(schema), defaultValues: schema.getDefault() });
    const {
        register,
        handleSubmit,
        control,
        formState: { errors },
    } = formMethods;

    const { fields, append } = useFieldArray({ control, name: 'phases' });

    const [createSchedule, { isLoading }] = useCreateScheduleMutation();

    const onSubmit: SubmitHandler<IForm> = async (data) => {
        const phases: PhaseInfo[] = [];
        let i = 0;

        for (const p of data.phases) {
            const { addRepeatingAtThatDay, durationFromLastPhase, durationType, ...mapProperties } = p;

            const seconds = DurationTypeToSeconds(p.durationFromLastPhase, p.durationType);

            const phase: PhaseInfo = {
                ...mapProperties,
                id: (i + 1).toString(),
                secondsFromLastPhase: seconds,
            };

            phases.push(phase);

            i++;

            if (!p.addRepeatingAtThatDay) {
                continue;
            }

            const repeatingPhase: PhaseInfo = {
                id: (i + 1).toString(),
                secondsFromLastPhase: 1,
                shortDescription: p.shortDescription,
                description: p.description,
                isDefaultValueSide: true,
            };

            phases.push(repeatingPhase);

            i++;
        }

        const { phases: _, ...scheduleMapProperties } = data;

        const schedule: CreateScheduleItem = {
            ...scheduleMapProperties,
            phases: phases,
        };

        try {
            await createSchedule(schedule).unwrap();
            props.onClose();
        } catch {}
    };

    return (
        <Dialog open={props.open} onClose={props.onClose} maxWidth={'sm'} fullWidth>
            <DialogTitle>Создание учебного плана</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form onSubmit={handleSubmit(onSubmit)}>
                        <FormField
                            label="Название"
                            error={!!errors.title}
                            errorMessage={errors.title?.message || ' '}
                            {...register('title')}
                        />
                        <FormField
                            label="Кол-во карт"
                            type="number"
                            error={!!errors.cardsCountPerPhase}
                            errorMessage={errors.cardsCountPerPhase?.message || ' '}
                            {...register('cardsCountPerPhase')}
                        />
                        <FormField
                            label="Краткое описание при старте"
                            error={!!errors.shortDescription}
                            errorMessage={errors.shortDescription?.message || ' '}
                            {...register('shortDescription')}
                        />
                        <FormField
                            label="Описание при старте"
                            error={!!errors.description}
                            errorMessage={errors.description?.message || ' '}
                            {...register('description')}
                        />
                        <FormField
                            label="Краткое описание для всех фаз"
                            error={!!errors.defaultPhaseShortDescription}
                            errorMessage={errors.defaultPhaseShortDescription?.message || ' '}
                            {...register('defaultPhaseShortDescription')}
                        />
                        <FormField
                            label="Описание для всех фаз"
                            error={!!errors.defaultPhaseDescription}
                            errorMessage={errors.defaultPhaseDescription?.message || ' '}
                            {...register('defaultPhaseDescription')}
                        />
                        <FormField
                            label="Краткое описание для фаз повторения"
                            error={!!errors.defaultRepeatPhaseShortDescription}
                            errorMessage={errors.defaultRepeatPhaseShortDescription?.message || ' '}
                            {...register('defaultRepeatPhaseShortDescription')}
                        />
                        <FormField
                            label="Описание для фаз повторения"
                            error={!!errors.defaultRepeatPhaseDescription}
                            errorMessage={errors.defaultRepeatPhaseDescription?.message || ' '}
                            {...register('defaultRepeatPhaseDescription')}
                        />
                        <FormFiledLabel label="При забывании" htmlFor="title-input">
                            <ForgottenBehaviorSelect registerName="forgottenBehavior" />
                        </FormFiledLabel>
                        <div style={{ marginTop: 20, display: 'flex', flexDirection: 'column' }}>
                            {fields.map((f, i) => (
                                <div key={f.id}>
                                    <Typography variant="h6">Этап {i + 1}</Typography>
                                    <div style={{ display: 'flex', alignItems: 'center', columnGap: 10 }}>
                                        <FormField
                                            label="Прошло с прошлого этапа"
                                            type="number"
                                            error={!!errors.phases?.at(i)?.durationFromLastPhase}
                                            errorMessage={errors.phases?.at(i)?.durationFromLastPhase?.message || ' '}
                                            {...register(`phases.${i}.durationFromLastPhase`)}
                                        />
                                        <Select
                                            defaultValue={f.durationType}
                                            size="small"
                                            sx={{ width: 100 }}
                                            {...register(`phases.${i}.durationType`)}
                                        >
                                            <MenuItem value={DurationType.Seconds}>Сек</MenuItem>
                                            <MenuItem value={DurationType.Minutes}>Мин</MenuItem>
                                            <MenuItem value={DurationType.Hours}>Час</MenuItem>
                                            <MenuItem value={DurationType.Days}>Дн</MenuItem>
                                        </Select>
                                    </div>
                                    <FormField
                                        label={'Переопределить краткое описание'}
                                        error={!!errors.phases?.at(i)?.shortDescription}
                                        errorMessage={errors.phases?.at(i)?.shortDescription?.message || ' '}
                                        {...register(`phases.${i}.shortDescription`)}
                                    />
                                    <FormField
                                        label={'Переопределить описание'}
                                        error={!!errors.phases?.at(i)?.description}
                                        errorMessage={errors.phases?.at(i)?.description?.message || ' '}
                                        {...register(`phases.${i}.description`)}
                                    />
                                    <FormControlLabel
                                        label="Повторение после изучения"
                                        control={
                                            <Checkbox
                                                checked={f.addRepeatingAtThatDay}
                                                {...register(`phases.${i}.addRepeatingAtThatDay`)}
                                            />
                                        }
                                    />
                                    <FormControlLabel
                                        label="Показывать только значение"
                                        control={
                                            <Checkbox
                                                checked={f.isDefaultValueSide}
                                                {...register(`phases.${i}.isDefaultValueSide`)}
                                            />
                                        }
                                    />
                                </div>
                            ))}
                        </div>
                        <Button onClick={() => append(phaseSchema.getDefault())}>Add</Button>
                        <Button disabled={isLoading} type="submit">
                            Создать
                        </Button>
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions></DialogActions>
        </Dialog>
    );
};
