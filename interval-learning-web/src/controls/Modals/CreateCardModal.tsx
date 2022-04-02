import { yupResolver } from '@hookform/resolvers/yup';
import { ArrowForwardIos } from '@mui/icons-material';
import { Dialog, DialogTitle, DialogContent, DialogActions, Button } from '@mui/material';
import { FC } from 'react';
import { FieldError, FormProvider, SubmitHandler, useFieldArray, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { CreateCardItem, useAddCardMutation } from '../../redux/cardsApi';
import { Schedule } from '../../types/schedule';
import { Form, FormField, FormFiledLabel, IconFormField } from '../Form/Form';
import { SelectSchedule } from '../SelectSchedule/SelectSchedule';

interface Example {
    value: string;
}

interface CardForm {
    frontText: string;
    backText: string;
    schedule: Schedule;
    description: string | null;
    examples: Example[];
}

const exampleSchema = yup.object({
    value: yup.string().max(255),
});

const schema = yup
    .object({
        frontText: yup.string().required().max(255),
        backText: yup.string().required().max(255),
        schedule: yup.object().required(),
        description: yup.string().max(500),
        examples: yup.array().of(exampleSchema),
    })
    .required();

interface CreateCardModalProps {
    open: boolean;
    onClose: () => void;
    collectionId: string;
    collectionUserId: string;
    defaultSchedule?: Schedule;
}

export const CreateCardModal: FC<CreateCardModalProps> = (props) => {
    const formMethods = useForm<CardForm>({
        resolver: yupResolver(schema),
        defaultValues: { schedule: props.defaultSchedule, examples: [{ value: '' }] },
    });
    const {
        register,
        handleSubmit,
        control,
        getValues,
        formState: { errors },
    } = formMethods;

    const { fields, append } = useFieldArray({ control, name: 'examples' });

    const [addCard, { isLoading }] = useAddCardMutation();

    const onAddExample = () => {
        const currentState = getValues();
        if (currentState.examples.every((e) => Boolean(e.value))) {
            append({ value: '' });
        }
    };

    const onCreate: SubmitHandler<CardForm> = async (data) => {
        const item: CreateCardItem = {
            frontText: data.frontText,
            backText: data.backText,
            scheduleUserId: data.schedule.userId,
            scheduleId: data.schedule.id,
            description: data.description,
            examples: data.examples.filter((e) => Boolean(e.value)).map((e) => e.value),
        };

        try {
            await addCard({ collectionId: props.collectionId, userId: props.collectionUserId, request: item }).unwrap();
            props.onClose();
        } catch {}
    };

    return (
        <Dialog open={props.open} onClose={props.onClose} maxWidth={'sm'} fullWidth>
            <DialogTitle>Создание карточки</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form>
                        <FormField
                            label="Запомнить"
                            error={!!errors.frontText}
                            errorMessage={errors.frontText?.message}
                            {...register('frontText')}
                        />
                        <FormField
                            label="Значение"
                            error={!!errors.backText}
                            errorMessage={errors.backText?.message}
                            {...register('backText')}
                        />
                        <SelectSchedule
                            label="Учебный план"
                            error={!!errors.schedule}
                            errorMessage={(errors.schedule as FieldError)?.message}
                            registeredName="schedule"
                        />
                        <FormField
                            label="Описание"
                            error={!!errors.description}
                            errorMessage={errors.description?.message}
                            {...register('description')}
                        />
                        <FormFiledLabel label="Примеры" />
                        <div style={{ display: 'flex', flexDirection: 'column' }}>
                            {fields.map((f, i) => {
                                console.log('f-' + i, f);
                                return (
                                    <div key={f.id}>
                                        <IconFormField
                                            label=""
                                            icon={ArrowForwardIos}
                                            error={!!errors.examples?.at(i)?.value}
                                            errorMessage={errors.examples?.at(i)?.value?.message}
                                            {...register(`examples.${i}.value`)}
                                        />
                                    </div>
                                );
                            })}
                        </div>
                        <Button onClick={onAddExample}>ADD</Button>
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions>
                <Button onClick={handleSubmit(onCreate)}>Создать</Button>
            </DialogActions>
        </Dialog>
    );
};
