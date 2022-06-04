import { yupResolver } from '@hookform/resolvers/yup';
import { Button, Typography, FormControlLabel, Checkbox, Divider, Stack, Tooltip } from '@mui/material';
import { FC } from 'react';
import { Controller, FormProvider, SubmitHandler, useFieldArray, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { UpdatePhaseInfo, UpdateScheduleRequest, useUpdateScheduleMutation } from '../../../redux/schedulesSlice';
import { PhaseInfo, Schedule } from '../../../types/schedule';
import { Form, FormField, FormFiledLabel, TextAreaFormField } from '../../../controls/Form/Form';
import { withMutationResolver, WithMutationResolverProps, withQueryResolver } from '../../../hoc/withQueryResolver';
import { useGetScheduleQuery } from '../../../redux/schedulesSlice';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { PageContent } from '../../../controls/PageContent/PageContent';
import { useNavigate, useParams } from 'react-router-dom';
import dayjs from 'dayjs';
import { Loop, Save } from '@mui/icons-material';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { getScheduleId, selectScheduleById } from '../../../redux/slices/scheduleSlice';
import { selectCurrentUser } from '../../../redux/currentUserSlice';

interface IPhaseForm {
    id: string;
    shortDescription: string | null;
    description: string | null;
    isDefaultValueSide: boolean;
    secondsFromLastStep: number;
    hasRepeatingPhase: boolean;
}

const phaseSchema = yup.object({
    shortDescription: yup.string().max(100),
    description: yup.string().max(1000),
    isDefaultValueSide: yup.boolean().default(false),
});

interface IForm {
    cardsCountPerPhase: number;
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
        phases: yup.array().of(phaseSchema).required(),
        shortDescription: yup.string().max(100),
        description: yup.string().max(1000),
        defaultPhaseShortDescription: yup.string().max(100),
        defaultRepeatPhaseShortDescription: yup.string().max(100),
        afterStartPhaseShortDescription: yup.string().max(100),
        defaultPhaseDescription: yup.string().max(1000),
        defaultRepeatPhaseDescription: yup.string().max(1000),
        afterStartPhaseDescription: yup.string().max(1000),
    })
    .required();

interface ScheduleEditPageContentProps extends WithMutationResolverProps<typeof useUpdateScheduleMutation> {
    scheduleId: string;
}

function getPhasesValues(phases: PhaseInfo[], hasRepeatingAfterStart: boolean): IPhaseForm[] {
    if (!phases || phases.length === 0) {
        return [];
    }

    const result: IPhaseForm[] = [];

    const startIndex = hasRepeatingAfterStart ? 1 : 0;

    for (let i = startIndex; i < phases.length; i++) {
        const phase = phases[i];
        const nextPhase = i + 1 < phases.length ? phases[i + 1] : undefined;

        const hasRepeatingPhase = Boolean(nextPhase);

        const form: IPhaseForm = {
            id: phase.id,
            hasRepeatingPhase,
            secondsFromLastStep: phase.secondsFromLastPhase,
            description: phase.description,
            shortDescription: phase.shortDescription,
            isDefaultValueSide: phase.isDefaultValueSide,
        };

        result.push(form);

        if (hasRepeatingPhase) {
            i++;
        }
    }

    return result;
}

function getScheduleValues(schedule: Schedule): IForm {
    const { phases: schedulePhases, ...propsToMatch } = schedule;
    const repeatAfterStart = schedule.phases?.length > 0 && schedule.phases[0].secondsFromLastPhase <= 10;
    const afterStartPhase = repeatAfterStart ? schedulePhases[0] : undefined;

    return {
        phases: getPhasesValues(schedulePhases, repeatAfterStart),
        repeatAfterStart,
        afterStartPhaseDescription: afterStartPhase?.shortDescription ?? '',
        afterStartPhaseShortDescription: afterStartPhase?.description ?? '',
        ...propsToMatch,
    };
}

const ScheduleEditPageContent: FC<ScheduleEditPageContentProps> = ({
    scheduleId,
    mutationProps: { mutate: updateSchedule, showRetryModal, isLoading },
}) => {
    const currentUser = useTypedSelector(selectCurrentUser);
    if (!currentUser) throw new Error();
    const schedule = useTypedSelector((state) => selectScheduleById(state, getScheduleId(currentUser?.id, scheduleId)));
    if (!schedule) throw new Error();

    const navigate = useNavigate();

    const formMethods = useForm<IForm>({
        resolver: yupResolver(schema),
        defaultValues: getScheduleValues(schedule),
    });

    const {
        register,
        handleSubmit,
        control,
        formState: { errors },
        watch,
    } = formMethods;

    const { fields } = useFieldArray({ control, name: 'phases' });

    const onSubmit: SubmitHandler<IForm> = async (data) => {
        const phases: UpdatePhaseInfo[] = [];

        const {
            phases: formPhases,
            repeatAfterStart,
            afterStartPhaseDescription,
            afterStartPhaseShortDescription,
            ...scheduleMapProperties
        } = data;

        let startIndex = 0;

        if (repeatAfterStart) {
            const repeatingPhaseId = formPhases[0].id;

            const repeatingPhase: UpdatePhaseInfo = {
                id: repeatingPhaseId,
                shortDescription: afterStartPhaseShortDescription,
                description: afterStartPhaseDescription,
                isDefaultValueSide: true,
            };

            phases.push(repeatingPhase);
            startIndex++;
        }

        for (let i = startIndex; i < formPhases.length; i++) {
            const p = formPhases[i];
            const { secondsFromLastStep, hasRepeatingPhase, ...mapProperties } = p;

            const phase: UpdatePhaseInfo = {
                ...mapProperties,
            };

            phases.push(phase);
        }

        const scheduleRequest: UpdateScheduleRequest = {
            scheduleId: schedule.id,
            data: {
                ...scheduleMapProperties,
                phases: phases,
            },
        };

        try {
            await updateSchedule(scheduleRequest);
            navigate('/schedules');
        } catch {
            showRetryModal(() => onSubmit(data));
        }
    };

    const labelWidth = '250px';

    const SaveButton = (
        <Button disabled={isLoading} variant={'contained'} onClick={handleSubmit(onSubmit)} endIcon={<Save />}>
            Сохранить
        </Button>
    );

    return (
        <PageContainer>
            <Controller
                control={control}
                name="title"
                render={({ field, fieldState, formState }) => (
                    <PageHeader title={field.value} onChange={field.onChange} editable subMenu={SaveButton} />
                )}
            />

            <PageContent>
                <FormProvider {...formMethods}>
                    <div style={{ margin: '16px 0' }}>
                        <Form>
                            <FormFiledLabel label="Рекомендуемое кол-во карт" labelWidth={labelWidth}>
                                <FormField
                                    error={!!errors.cardsCountPerPhase}
                                    errorMessage={errors.cardsCountPerPhase?.message || ' '}
                                    {...register('cardsCountPerPhase')}
                                />
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
                                            error={!!errors.afterStartPhaseDescription}
                                            errorMessage={errors.afterStartPhaseDescription?.message || ' '}
                                            {...register('afterStartPhaseDescription')}
                                            minRows={2}
                                        />
                                    </FormFiledLabel>
                                </>
                            )}
                            <Divider />
                            <div style={{ marginTop: 20, display: 'flex', flexDirection: 'column' }}>
                                {fields.map((f, i) => {
                                    const duration = dayjs.duration(f.secondsFromLastStep, 's');
                                    return (
                                        <div key={f.id}>
                                            <Stack direction="row" columnGap={'5px'} alignItems="center">
                                                <Typography variant="h6">Интервал: {duration.humanize()}</Typography>
                                                {f.hasRepeatingPhase && (
                                                    <Tooltip title={'Повторять после изучения'} placement="top-start">
                                                        <Loop color="primary" />
                                                    </Tooltip>
                                                )}
                                            </Stack>
                                            <FormFiledLabel
                                                label="Переопределить краткое описание"
                                                labelWidth={labelWidth}
                                            >
                                                <TextAreaFormField
                                                    error={!!errors.phases?.at(i)?.shortDescription}
                                                    errorMessage={
                                                        errors.phases?.at(i)?.shortDescription?.message || ' '
                                                    }
                                                    {...register(`phases.${i}.shortDescription`)}
                                                />
                                            </FormFiledLabel>
                                            <FormFiledLabel label="Переопределить описание" labelWidth={labelWidth}>
                                                <TextAreaFormField
                                                    error={!!errors.phases?.at(i)?.description}
                                                    errorMessage={errors.phases?.at(i)?.description?.message || ' '}
                                                    {...register(`phases.${i}.description`)}
                                                    minRows={2}
                                                />
                                            </FormFiledLabel>
                                            <FormControlLabel
                                                label="Показывать сначала значение (перевод)"
                                                control={
                                                    <Checkbox
                                                        defaultChecked={f.isDefaultValueSide}
                                                        // checked={f.isDefaultValueSide}
                                                        {...register(`phases.${i}.isDefaultValueSide`)}
                                                    />
                                                }
                                            />
                                            <Divider style={{ margin: '10px 0' }} />
                                        </div>
                                    );
                                })}
                            </div>
                        </Form>
                        <Stack direction={'row'} justifyContent="space-between">
                            <div />
                            {SaveButton}
                        </Stack>
                    </div>
                </FormProvider>
            </PageContent>
        </PageContainer>
    );
};

const WithEditMutation = withMutationResolver(
    useUpdateScheduleMutation,
    'Не удалось обновить учебный план'
)(ScheduleEditPageContent);

const WithScheduleLoading = withQueryResolver(useGetScheduleQuery)(WithEditMutation);

const LoadResolver: FC = () => {
    const { scheduleId } = useParams();

    if (!scheduleId) {
        throw new Error();
    }

    return <WithScheduleLoading queryArg={{ scheduleId }} />;
};

export const ScheduleEditPage = LoadResolver;
