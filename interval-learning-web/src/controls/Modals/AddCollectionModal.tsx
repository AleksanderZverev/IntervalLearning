import { yupResolver } from '@hookform/resolvers/yup';
import {
    Button,
    Checkbox,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
    FormControlLabel,
    MenuItem,
    Select,
} from '@mui/material';
import { FC, useMemo } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import * as yup from 'yup';
import {
    withMutationResolver,
    WithMutationResolverProps,
    withQueryResolver,
    WithQueryResolverData,
} from '../../hoc/withQueryResolver';
import {
    AddCardsToMyCollectionRequest,
    useAddCardsToMyCollectionMutation,
    useGetCollectionsQuery,
} from '../../redux/collectionApi';
import { StoreCollection } from '../../types/Collection';
import { Form, FormField } from '../Form/Form';

interface IForm {
    checkUnique: boolean;
    collectionId: string | undefined;
    newCollectionName: string | undefined;
}

const createNewValue = 'create-new';

const schema = yup.object({
    collectionId: yup.string().required().default(createNewValue),
    newCollectionName: yup
        .string()
        .when('collectionId', { is: createNewValue, then: (s) => s.required('Введите имя коллекции') }),
    checkUnique: yup.boolean(),
});

const defaultValues = schema.getDefault();

type Resolvers = WithQueryResolverData<typeof useGetCollectionsQuery> &
    WithMutationResolverProps<typeof useAddCardsToMyCollectionMutation>;

interface AddCollectionModalProps {
    open: boolean;
    onClose: () => void;
    collection: StoreCollection;
    onAdded: () => void;
}

type AddCollectionModalContentProps = AddCollectionModalProps & Resolvers;

const AddCollectionModalContent: FC<AddCollectionModalContentProps> = ({
    open,
    onClose,
    onAdded,
    collection,
    queryData: myCollections,
    mutationProps: { mutate: addCardsToCollection, showRetryModal },
}) => {
    const formMethods = useForm<IForm>({ resolver: yupResolver(schema), defaultValues: defaultValues });
    const {
        register,
        handleSubmit,
        watch,
        formState: { errors },
    } = formMethods;

    const collectionsToSelect = useMemo(
        () => myCollections.filter((c) => c.themeId === collection.themeId),
        [myCollections, collection]
    );

    const onSubmit: SubmitHandler<IForm> = async (data: IForm) => {
        const isCreatingNew = data.collectionId === createNewValue;

        const request: AddCardsToMyCollectionRequest = {
            publicCollectionUserId: collection.userId,
            publicCollectionId: collection.id,
            data: {
                checkUnique: data.checkUnique,
                myCollectionId: isCreatingNew ? undefined : data.collectionId,
                newCollectionName: isCreatingNew ? data.newCollectionName : undefined,
            },
        };

        try {
            await addCardsToCollection(request);
            onAdded();
            onClose();
        } catch {
            showRetryModal(() => onSubmit(data));
        }
    };

    return (
        <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
            <DialogTitle>Добавление: {collection.title}</DialogTitle>
            <DialogContent>
                <Form>
                    <Select {...register('collectionId')} defaultValue={defaultValues.collectionId} fullWidth>
                        <MenuItem key={'special'} value={createNewValue}>
                            Добавить в новую коллекцию
                        </MenuItem>
                        <Divider />
                        {collectionsToSelect.map((c) => (
                            <MenuItem key={c.id} value={c.id}>
                                {c.title}
                            </MenuItem>
                        ))}
                    </Select>
                    {watch('collectionId') === createNewValue && (
                        <FormField
                            label="Имя коллекции"
                            {...register('newCollectionName')}
                            error={Boolean(errors.newCollectionName)}
                            errorMessage={errors.newCollectionName?.message}
                        />
                    )}
                    <FormControlLabel
                        label="Исключить похожие"
                        control={<Checkbox {...register('checkUnique')} defaultChecked={defaultValues.checkUnique} />}
                    />
                </Form>
            </DialogContent>
            <DialogActions>
                <Button variant="contained" onClick={handleSubmit(onSubmit)}>
                    Добавить
                </Button>
            </DialogActions>
        </Dialog>
    );
};

const WithCollections = withQueryResolver(useGetCollectionsQuery)(AddCollectionModalContent);

const WithAddCards = withMutationResolver(
    useAddCardsToMyCollectionMutation,
    'Не удалось добавить коллекцию'
)(WithCollections);

export const AddCollectionModal: FC<AddCollectionModalProps> = (props) => {
    return <WithAddCards {...props} queryArg={undefined} />;
};
