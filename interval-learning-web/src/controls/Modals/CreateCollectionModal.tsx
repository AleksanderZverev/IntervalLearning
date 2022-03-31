import { Button, Dialog, DialogActions, DialogContent, DialogTitle } from '@mui/material';
import { FC } from 'react';
import { Theme } from '../../types/global';
import { SelectTheme } from '../SelectTheme/SelectTheme';
import * as yup from 'yup';
import { Schedule } from '../../types/schedule';
import { FormProvider, SubmitHandler, useForm, FieldError } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Form, FormField } from '../Form/Form';
import { SelectSchedule } from '../SelectSchedule/SelectSchedule';
import { CreateCollectionItem, useCreateCollectionMutation } from '../../redux/collectionApi';

interface IForm {
    title: string;
    theme: Theme;
    schedule: Schedule;
}

const schema = yup
    .object({
        title: yup.string().max(100).required(),
        theme: yup.object().required(),
        schedule: yup.object().required(),
    })
    .required();

interface CreateCollectionModalProps {
    open: boolean;
    onClose: () => void;
}

export const CreateCollectionModal: FC<CreateCollectionModalProps> = (props) => {
    const formMethods = useForm<IForm>({ resolver: yupResolver(schema) });
    const {
        handleSubmit,
        register,
        formState: { errors },
    } = formMethods;

    const [createCollection, {}] = useCreateCollectionMutation();

    const onCreate: SubmitHandler<IForm> = async (data) => {
        const item: CreateCollectionItem = {
            title: data.title,
            themeId: data.theme.id,
            scheduleId: data.schedule.id,
            scheduleUserId: data.schedule.userId,
            isDefaultBackSide: false,
        };
        try {
            await createCollection(item).unwrap();
            props.onClose();
        } catch {}
    };
    return (
        <Dialog open={props.open} onClose={props.onClose} maxWidth={'xs'} sx={{ minWidth: 400 }} fullWidth>
            <DialogTitle sx={{ fontSize: 32 }}>Создание коллекции</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form>
                        <FormField
                            label="Название"
                            error={!!errors.title}
                            errorMessage={errors.title?.message}
                            {...register('title')}
                        />
                        <SelectTheme
                            label="Тема"
                            error={!!errors.theme}
                            errorMessage={(errors.theme as FieldError)?.message}
                            registeredName="theme"
                        />
                        <SelectSchedule
                            label="Учебный план"
                            error={!!errors.schedule}
                            errorMessage={(errors.schedule as FieldError)?.message}
                            registeredName="schedule"
                        />
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions>
                <div style={{ margin: 15 }}>
                    <Button variant="outlined" onClick={handleSubmit(onCreate)}>
                        Создать
                    </Button>
                </div>
            </DialogActions>
        </Dialog>
    );
};
