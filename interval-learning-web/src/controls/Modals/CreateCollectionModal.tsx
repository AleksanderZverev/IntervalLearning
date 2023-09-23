import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack } from '@mui/material';
import { FC } from 'react';
import { Theme } from '../../types/global';
import { SelectThemeControl } from '../SelectTheme/SelectTheme';
import * as yup from 'yup';
import { FormProvider, SubmitHandler, useForm, FieldError } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Form, FormField } from '../Form/Form';
import {
    CreateCollectionItem,
    MakePublicRequest,
    useCreateCollectionMutation,
    useMakeCollectionPublicMutation,
} from '../../redux/collectionApi';
import useTypedSelector from '../../hooks/useTypedSelector';
import { selectCollectionById } from '../../redux/slices/collectionsSlice';
import { Collection } from '../../types/Collection';
import { selectTheme } from '../../redux/slices/themeSlice';
import { withMutationResolver, WithMutationResolverProps } from '../../hoc/withQueryResolver';

interface IForm {
    title: string;
    theme: Theme;
}

const schema = yup
    .object({
        title: yup.string().max(100).required(),
        theme: yup.object().required(),
    })
    .required();

interface CreateCollectionModalProps extends WithMutationResolverProps<typeof useCreateCollectionMutation> {
    collectionId?: string;
    userId?: string;
    open: boolean;
    onClose: () => void;
}

function getDefaultFormValue(collection: Collection, theme: Theme) {
    const form: IForm = {
        title: collection.title,
        theme,
    };

    return form;
}

const CreateCollectionModalContent: FC<CreateCollectionModalProps> = ({
    mutationProps: { mutate: createCollection, showRetryModal },
    ...props
}) => {
    const collection = useTypedSelector((state) =>
        selectCollectionById(state, props.userId ?? '', props.collectionId ?? '')
    );

    const theme = useTypedSelector((state) => selectTheme(state, collection?.themeId ?? ''));

    if (collection && !theme) {
        throw new Error();
    }

    const [makePublic, { isSuccess: isMadePublic, isError: makingPublicError, isLoading: isMakingPublic }] =
        useMakeCollectionPublicMutation();

    const formMethods = useForm<IForm>({
        resolver: yupResolver(schema),
        defaultValues: collection && theme ? getDefaultFormValue(collection, theme) : undefined,
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
            isDefaultBackSide: false,
        };
        try {
            await createCollection(item);
            props.onClose();
        } catch {
            showRetryModal(() => onCreate(data));
        }
    };

    const onMakePublic = async () => {
        if (!props.collectionId || isMadePublic) {
            return;
        }

        const request: MakePublicRequest = {
            collectionId: props.collectionId,
        };

        await makePublic(request);
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
                            multiline
                            label="Название"
                            error={!!errors.title}
                            errorMessage={errors.title?.message}
                            {...register('title')}
                        />
                        <SelectThemeControl
                            label="Тема"
                            error={!!errors.theme}
                            errorMessage={(errors.theme as FieldError)?.message}
                            registeredName="theme"
                        />
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions>
                <div
                    style={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        margin: 15,
                        marginTop: 0,
                        width: '100%',
                    }}
                >
                    {collection && !collection.isPublic ? (
                        <Button variant="outlined" onClick={onMakePublic} disabled={isMakingPublic}>
                            Опубликовать
                        </Button>
                    ) : (collection && collection.isPublic) || isMadePublic ? (
                        <Button variant="outlined" color={'success'}>
                            Опубликована
                        </Button>
                    ) : (
                        <div />
                    )}
                    <Button variant="contained" onClick={handleSubmit(onCreate)}>
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
