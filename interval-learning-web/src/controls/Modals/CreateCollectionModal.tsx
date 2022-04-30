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
import useTypedSelector from '../../hooks/useTypedSelector';
import { selectCollectionById } from '../../redux/slices/collectionsSlice';
import { Collection } from '../../types/Collection';
import { selectTheme } from '../../redux/slices/themeSlice';
import { getScheduleId, selectScheduleById } from '../../redux/slices/scheduleSlice';
import { withMutationResolver, WithMutationResolverProps } from '../../hoc/withQueryResolver';

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

interface CreateCollectionModalProps extends WithMutationResolverProps<typeof useCreateCollectionMutation> {
    collectionId?: string;
    userId?: string;
    open: boolean;
    onClose: () => void;
}

function getDefaultFormValue(collection: Collection, theme: Theme, schedule: Schedule) {
    const form: IForm = {
        title: collection.title,
        theme,
        schedule,
    };

    return form;
}

const CreateCollectionModalContent: FC<CreateCollectionModalProps> = ({
    mutate: createCollection,
    showRetryModal,
    ...props
}) => {
    const collection = useTypedSelector((state) =>
        selectCollectionById(state, props.userId ?? '', props.collectionId ?? '')
    );
    const theme = useTypedSelector((state) => selectTheme(state, collection?.themeId ?? ''));
    const schedule = useTypedSelector((state) =>
        selectScheduleById(state, getScheduleId(collection?.userId ?? '', collection?.defaultScheduleId ?? ''))
    );

    if (collection && (!theme || !schedule)) {
        throw new Error();
    }

    const formMethods = useForm<IForm>({
        resolver: yupResolver(schema),
        defaultValues: collection && theme && schedule ? getDefaultFormValue(collection, theme, schedule) : undefined,
    });
    const {
        handleSubmit,
        register,
        formState: { errors },
    } = formMethods;

    const onCreate: SubmitHandler<IForm> = async (data) => {
        const item: CreateCollectionItem = {
            collectionId: props.collectionId,
            title: data.title,
            themeId: data.theme.id,
            scheduleId: data.schedule.id,
            scheduleUserId: data.schedule.userId,
            isDefaultBackSide: false,
        };
        try {
            await createCollection(item);
            props.onClose();
        } catch {
            showRetryModal(() => onCreate(data));
        }
    };

    return (
        <Dialog open={props.open} onClose={props.onClose} maxWidth={'xs'} sx={{ minWidth: 400 }} fullWidth>
            <DialogTitle sx={{ fontSize: 32 }}>
                {props.collectionId ? 'Изменение коллекции' : 'Создание коллекции'}
            </DialogTitle>
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
                        {props.collectionId ? 'Сохранить' : 'Создать'}
                    </Button>
                </div>
            </DialogActions>
        </Dialog>
    );
};

export const CreateCollectionModal = withMutationResolver(
    useCreateCollectionMutation,
    'Не удалось создать коллекцию'
)(CreateCollectionModalContent);
