import { yupResolver } from '@hookform/resolvers/yup';
import { Dialog, DialogTitle, DialogContent, DialogActions, Button } from '@mui/material';
import { FC } from 'react';
import { FieldError, FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { Schedule } from '../../types/schedule';
import { Form, FormField, FormFiledLabel } from '../Form/Form';
import { SelectSchedule } from '../SelectSchedule/SelectSchedule';

interface IForm {
    frontText: string;
    backText: string;
    schedule: Schedule;
    description: string | null;
    examples: string[] | null;
}

const schema = yup
    .object({
        frontText: yup.string().required().max(255),
        backText: yup.string().required().max(255),
        schedule: yup.object().required(),
        description: yup.string().max(500).default(null),
        examples: yup.array(yup.string().max(255)).default([]),
    })
    .required();

interface CreateCardModalProps {
    open: boolean;
    onClose: () => void;
    defaultSchedule?: Schedule;
}

export const CreateCardModal: FC<CreateCardModalProps> = (props) => {
    const formMethods = useForm<IForm>({
        resolver: yupResolver(schema),
        defaultValues: { schedule: props.defaultSchedule },
    });
    const {
        register,
        handleSubmit,
        formState: { errors },
    } = formMethods;

    const onCreate: SubmitHandler<IForm> = (data) => {
        console.log('ok', data);
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
                        <FormFiledLabel label="Примеры"></FormFiledLabel>
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions>
                <Button onClick={handleSubmit(onCreate)}>Создать</Button>
            </DialogActions>
        </Dialog>
    );
};
