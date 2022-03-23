import { yupResolver } from '@hookform/resolvers/yup';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormLabel,
    MenuItem,
    Select,
    TextField,
    Typography,
} from '@mui/material';
import { FC, useState } from 'react';
import { FormProvider, SubmitHandler, useFieldArray, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { CreateScheduleItem, ForgottenBehavior, PhaseInfo } from '../../types/schedule';
import { ForgottenBehaviorSelect } from '../ForgottenBehaviorSelect/ForgottenBehaviorSelect';
import { Form, FormField } from '../Form/Form';

enum DurationType {
    Seconds = 1,
    Minutes = 2,
    Hours = 3,
    Days = 4,
}

function DurationTypeToSeconds(value: number, type: DurationType) {
    let result = value;
    let resultType = type;

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
    description: string | null;
}

const defaultDuration = DurationType.Days;

const phaseSchema = yup.object({
    durationFromLastPhase: yup.number().min(1).required(),
    durationType: yup.number().required().default(defaultDuration),
    description: yup.string().max(500),
});

interface IForm {
    cardsCountPerPhase: number;
    forgottenBehavior: ForgottenBehavior;
    title: string;
    description: string | null;
    phases: IPhaseForm[];
}

const schema = yup
    .object({
        cardsCountPerPhase: yup.number().min(0).max(9999).required(),
        title: yup.string().min(1).max(255).required(),
        forgottenBehavior: yup.number().required().default(1),
        phases: yup.array().of(phaseSchema).required(),
        description: yup.string().max(500),
    })
    .required();

interface CreateScheduleModalProps {
    open: boolean;
    onClose: () => void;
}

export const CreateScheduleModal: FC<CreateScheduleModalProps> = (props) => {
    const formMethods = useForm<IForm>({ resolver: yupResolver(schema) });
    const {
        register,
        handleSubmit,
        control,
        formState: { errors },
    } = formMethods;

    const { fields, append } = useFieldArray({ control, name: 'phases' });

    const onSubmit: SubmitHandler<IForm> = (data) => {
        const schedule: CreateScheduleItem = {
            cardsCountPerPhase: data.cardsCountPerPhase,
            forgottenBehavior: data.forgottenBehavior,
            title: data.title,
            description: data.description,
            phases: data.phases.map((p) => {
                const seconds = DurationTypeToSeconds(p.durationFromLastPhase, p.durationType);
                const phase: PhaseInfo = {
                    secondsFromLastPhase: seconds,
                    description: p.description,
                };

                return phase;
            }),
        };

        console.log('schedule', schedule);
    };

    return (
        <Dialog open={props.open} onClose={props.onClose} maxWidth={'sm'} fullWidth>
            <DialogTitle>Создание учебного плана</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form onSubmit={handleSubmit(onSubmit)}>
                        <FormField label="Название" htmlFor="title-input">
                            <TextField
                                id="title-input"
                                size="small"
                                fullWidth
                                error={!!errors.title}
                                helperText={errors.title?.message || ' '}
                                {...register('title')}
                            />
                        </FormField>
                        <FormField label="Кол-во карт" htmlFor="card-number">
                            <TextField
                                id="card-number"
                                type="number"
                                size="small"
                                error={!!errors.cardsCountPerPhase}
                                helperText={errors.cardsCountPerPhase?.message || ' '}
                                {...register('cardsCountPerPhase')}
                            />
                        </FormField>
                        <FormField label="Описание" htmlFor="desc">
                            <TextField
                                id="desc"
                                size="small"
                                error={!!errors.description}
                                helperText={errors.description?.message || ' '}
                                {...register('description')}
                            />
                        </FormField>
                        <FormField label="Действие при забывании" htmlFor="title-input">
                            <ForgottenBehaviorSelect registerName="forgottenBehavior" />
                        </FormField>
                        <div style={{ marginTop: 20, display: 'flex', flexDirection: 'column' }}>
                            {fields.map((f, i) => (
                                <div key={f.id}>
                                    <Typography variant="h6">Phase {i + 1}</Typography>
                                    <FormField label={'Секунд с прошлого этапа'} htmlFor={'input-desc-' + i}>
                                        <TextField
                                            id={'input-desc-' + i}
                                            type={'number'}
                                            size="small"
                                            error={!!errors.phases?.at(i)?.durationFromLastPhase}
                                            helperText={errors.phases?.at(i)?.durationFromLastPhase?.message || ' '}
                                            {...register(`phases.${i}.durationFromLastPhase`)}
                                        />
                                        <Select
                                            defaultValue={defaultDuration}
                                            size="small"
                                            sx={{ width: 85 }}
                                            {...register(`phases.${i}.durationType`)}
                                        >
                                            <MenuItem value={DurationType.Seconds}>Сек</MenuItem>
                                            <MenuItem value={DurationType.Minutes}>Мин</MenuItem>
                                            <MenuItem value={DurationType.Hours}>Час</MenuItem>
                                            <MenuItem value={DurationType.Days}>Дн</MenuItem>
                                        </Select>
                                    </FormField>
                                    <FormField label={'Описание'} htmlFor={'desc-' + i}>
                                        <TextField
                                            id={'desc-' + i}
                                            size="small"
                                            error={!!errors.phases?.at(i)?.description}
                                            helperText={errors.phases?.at(i)?.description?.message || ' '}
                                            {...register(`phases.${i}.description`)}
                                        />
                                    </FormField>
                                </div>
                            ))}
                        </div>
                        <Button onClick={() => append({ durationFromLastPhase: 0, description: '' })}>Add</Button>
                        <Button type="submit">Создать</Button>
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions></DialogActions>
        </Dialog>
    );
};
