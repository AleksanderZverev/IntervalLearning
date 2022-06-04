import { yupResolver } from '@hookform/resolvers/yup';
import {
    Button,
    MenuItem,
    Select,
    Typography,
    FormControlLabel,
    Checkbox,
    Divider,
    IconButton,
    Stack,
} from '@mui/material';
import { FC } from 'react';
import { FormProvider, SubmitHandler, useFieldArray, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { useCreateScheduleMutation } from '../../../redux/schedulesSlice';
import { CreateScheduleItem, ForgottenBehavior, PhaseInfo, Schedule } from '../../../types/schedule';
import { ForgottenBehaviorSelect } from '../../../controls/ForgottenBehaviorSelect/ForgottenBehaviorSelect';
import { Form, FormField, FormFiledLabel, TextAreaFormField } from '../../../controls/Form/Form';
import { withMutationResolver, WithMutationResolverProps } from '../../../hoc/withQueryResolver';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { PageContent } from '../../../controls/PageContent/PageContent';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import { Add, Delete } from '@mui/icons-material';

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
    repeatAfterStart: boolean;
    afterStartPhaseShortDescription: string | null;
    afterStartPhaseDescription: string | null;
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
        defaultRepeatPhaseShortDescription: yup.string().max(100),
        afterStartPhaseShortDescription: yup.string().max(100),
        defaultPhaseDescription: yup.string().max(1000),
        defaultRepeatPhaseDescription: yup.string().max(1000),
        afterStartPhaseDescription: yup.string().max(1000),
        repeatAfterStart: yup.boolean().default(true),
    })
    .required();

interface ScheduleCreatePageContentProps extends WithMutationResolverProps<typeof useCreateScheduleMutation> {}

const ScheduleCreatePageContent: FC<ScheduleCreatePageContentProps> = ({
    mutationProps: { mutate: createSchedule, isLoading, showRetryModal },
}) => {
    const navigate = useNavigate();

    const formMethods = useForm<IForm>({
        resolver: yupResolver(schema),
        defaultValues: schema.getDefault(),
    });
    const {
        register,
        handleSubmit,
        control,
        formState: { errors },
        watch,
    } = formMethods;

    const { fields, append, remove } = useFieldArray({ control, name: 'phases' });

    const onSubmit: SubmitHandler<IForm> = async (data) => {
        const phases: PhaseInfo[] = [];
        let i = 0;

        const {
            phases: formPhases,
            repeatAfterStart,
            afterStartPhaseDescription,
            afterStartPhaseShortDescription,
            ...scheduleMapProperties
        } = data;

        const addRepeatPhase = (shortDescription: string | null, description: string | null) => {
            const repeatingPhase: PhaseInfo = {
                id: (i + 1).toString(),
                secondsFromLastPhase: 1,
                shortDescription,
                description,
                isDefaultValueSide: true,
            };

            phases.push(repeatingPhase);

            i++;
        };

        if (repeatAfterStart) {
            addRepeatPhase(afterStartPhaseShortDescription, afterStartPhaseDescription);
        }

        for (const p of formPhases) {
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

            addRepeatPhase(p.shortDescription, p.description);
        }

        const schedule: CreateScheduleItem = {
            ...scheduleMapProperties,
            phases: phases,
        };

        try {
            await createSchedule(schedule);
            navigate('/schedules');
        } catch {
            showRetryModal(() => onSubmit(data));
        }
    };

    const labelWidth = '250px';

    const createButton = (
        <Button variant="contained" onClick={handleSubmit(onSubmit)} disabled={isLoading}>
            Создать
        </Button>
    );

    return (
        <PageContainer>
            <PageHeader title="Создание учебного плана" subMenu={createButton} />
            <PageContent>
                <div style={{ margin: '16px 0' }}>
                    <FormProvider {...formMethods}>
                        <Form onSubmit={handleSubmit(onSubmit)}>
                            <FormFiledLabel label="Название" labelWidth={labelWidth}>
                                <FormField
                                    error={!!errors.title}
                                    errorMessage={errors.title?.message || ' '}
                                    {...register('title')}
                                />
                            </FormFiledLabel>
                            <FormFiledLabel label="Рекомендуемое кол-во карт" labelWidth={labelWidth}>
                                <FormField
                                    error={!!errors.cardsCountPerPhase}
                                    errorMessage={errors.cardsCountPerPhase?.message || ' '}
                                    {...register('cardsCountPerPhase')}
                                />
                            </FormFiledLabel>
                            <FormFiledLabel
                                label="При забывании"
                                htmlFor="title-input"
                                justifyContent="flex-start"
                                labelWidth={labelWidth}
                            >
                                <ForgottenBehaviorSelect registerName="forgottenBehavior" />
                            </FormFiledLabel>
                            <FormFiledLabel label="Краткое описание при старте" labelWidth={labelWidth}>
                                <TextAreaFormField
                                    error={!!errors.shortDescription}
                                    errorMessage={errors.shortDescription?.message || ' '}
                                    {...register('shortDescription')}
                                />
                            </FormFiledLabel>
                            <FormFiledLabel label="Описание при старте" labelWidth={labelWidth}>
                                <TextAreaFormField
                                    error={!!errors.description}
                                    errorMessage={errors.description?.message || ' '}
                                    {...register('description')}
                                    minRows={2}
                                />
                            </FormFiledLabel>
                            <FormFiledLabel label="Краткое описание для всех фаз" labelWidth={labelWidth}>
                                <TextAreaFormField
                                    error={!!errors.defaultPhaseShortDescription}
                                    errorMessage={errors.defaultPhaseShortDescription?.message || ' '}
                                    {...register('defaultPhaseShortDescription')}
                                />
                            </FormFiledLabel>
                            <FormFiledLabel label="Описание для всех фаз" labelWidth={labelWidth}>
                                <TextAreaFormField
                                    error={!!errors.defaultPhaseDescription}
                                    errorMessage={errors.defaultPhaseDescription?.message || ' '}
                                    {...register('defaultPhaseDescription')}
                                    minRows={2}
                                />
                            </FormFiledLabel>
                            <FormFiledLabel label="Краткое описание для фаз повторения" labelWidth={labelWidth}>
                                <TextAreaFormField
                                    error={!!errors.defaultRepeatPhaseShortDescription}
                                    errorMessage={errors.defaultRepeatPhaseShortDescription?.message || ' '}
                                    {...register('defaultRepeatPhaseShortDescription')}
                                />
                            </FormFiledLabel>
                            <FormFiledLabel label="Описание для фаз повторения" labelWidth={labelWidth}>
                                <TextAreaFormField
                                    error={!!errors.defaultRepeatPhaseDescription}
                                    errorMessage={errors.defaultRepeatPhaseDescription?.message || ' '}
                                    {...register('defaultRepeatPhaseDescription')}
                                    minRows={2}
                                />
                            </FormFiledLabel>
                            <FormControlLabel
                                label={'Повторение после старта'}
                                control={
                                    <Checkbox checked={watch('repeatAfterStart')} {...register(`repeatAfterStart`)} />
                                }
                            />
                            {watch('repeatAfterStart') && (
                                <>
                                    <FormFiledLabel
                                        label="Краткое описание для повторения после старта"
                                        labelWidth={labelWidth}
                                    >
                                        <TextAreaFormField
                                            error={!!errors.afterStartPhaseShortDescription}
                                            errorMessage={errors.afterStartPhaseShortDescription?.message || ' '}
                                            {...register('afterStartPhaseShortDescription')}
                                        />
                                    </FormFiledLabel>
                                    <FormFiledLabel
                                        label="Описание для повторения после старта"
                                        labelWidth={labelWidth}
                                    >
                                        <TextAreaFormField
                                            // label="Описание для повторения после старта"
                                            error={!!errors.afterStartPhaseDescription}
                                            errorMessage={errors.afterStartPhaseDescription?.message || ' '}
                                            {...register('afterStartPhaseDescription')}
                                            minRows={2}
                                        />
                                    </FormFiledLabel>
                                </>
                            )}
                            <div style={{ marginTop: 20, display: 'flex', flexDirection: 'column' }}>
                                {fields.map((f, i) => (
                                    <div key={f.id} style={{ position: 'relative' }}>
                                        <Divider style={{ margin: '10px 0' }} />
                                        <IconButton
                                            sx={{ position: 'absolute', top: '15px', right: '5px' }}
                                            onClick={() => remove(i)}
                                        >
                                            <Delete color="error" />
                                        </IconButton>
                                        <Typography variant="h6">Этап {i + 1}</Typography>
                                        <div style={{ display: 'flex', alignItems: 'center', columnGap: 10 }}>
                                            <FormField
                                                label="Прошло с прошлого этапа"
                                                type="number"
                                                error={!!errors.phases?.at(i)?.durationFromLastPhase}
                                                errorMessage={
                                                    errors.phases?.at(i)?.durationFromLastPhase?.message || ' '
                                                }
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
                        </Form>
                    </FormProvider>

                    <Divider style={{ margin: '10px 0' }} />
                    <Stack direction={'row'} justifyContent="space-between">
                        <Button
                            variant="outlined"
                            onClick={() => append(phaseSchema.getDefault())}
                            endIcon={<Add color="primary" />}
                        >
                            Новый этап
                        </Button>
                        {createButton}
                    </Stack>
                </div>
            </PageContent>
        </PageContainer>
    );
};

const WithCreateScheduleMutation = withMutationResolver(
    useCreateScheduleMutation,
    'Не удалось создать учебный план'
)(ScheduleCreatePageContent);

export const ScheduleCreatePage = WithCreateScheduleMutation;
