import { yupResolver } from '@hookform/resolvers/yup';
import { ArrowForwardIos } from '@mui/icons-material';
import { Dialog, DialogTitle, DialogContent, DialogActions, Button } from '@mui/material';
import { FC } from 'react';
import { FormProvider, SubmitHandler, useFieldArray, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { withMutationResolver, WithMutationResolverProps } from '../../hoc/withQueryResolver';
import useTypedSelector from '../../hooks/useTypedSelector';
import { CreateCardItem, useAddCardMutation } from '../../redux/cardsApi';
import { selectCardById } from '../../redux/slices/cardsSlice';
import { Card } from '../../types/Collection';
import { Form, FormField, FormFiledLabel, IconFormField } from '../Form/Form';

interface Example {
    value: string;
}

interface CardForm {
    frontText: string;
    promptText: string | null;
    backText: string;
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
        description: yup.string().max(500),
        examples: yup.array().of(exampleSchema),
    })
    .required();

interface CreateCardModalProps extends WithMutationResolverProps<typeof useAddCardMutation> {
    collectionId: string;
    collectionUserId: string;
    open: boolean;
    onClose: () => void;
    cardId?: string;
    // defaultSchedule?: Schedule;
}

function getDefaultValues(card: Card): CardForm {
    const examples = card.examples?.map((e) => ({ value: e })) ?? [];
    examples.push({ value: '' });

    const cardForm: CardForm = {
        frontText: card.frontSideText,
        promptText: card.promptText,
        backText: card.backSideText,
        description: card.description,
        examples,
    };

    return cardForm;
}

const CreateCardModalContent: FC<CreateCardModalProps> = ({
    mutationProps: { mutate: addCard, showRetryModal, isLoading },
    ...props
}) => {
    const card = useTypedSelector((state) =>
        selectCardById(state, props.collectionUserId, props.collectionId, props.cardId)
    );

    const formMethods = useForm<CardForm>({
        resolver: yupResolver(schema),
        defaultValues: card ? getDefaultValues(card) : { examples: [{ value: '' }] },
    });

    const {
        register,
        handleSubmit,
        control,
        getValues,
        formState: { errors },
    } = formMethods;

    const { fields, append } = useFieldArray({ control, name: 'examples' });

    const onAddExample = () => {
        const currentState = getValues();
        if (currentState.examples.every((e) => Boolean(e.value))) {
            append({ value: '' });
        }
    };

    const onCreate: SubmitHandler<CardForm> = async (data) => {
        const item: CreateCardItem = {
            cardId: props.cardId,
            frontText: data.frontText,
            promptText: data.promptText,
            backText: data.backText,
            description: data.description,
            examples: data.examples.filter((e) => Boolean(e.value)).map((e) => e.value),
        };

        try {
            await addCard({ collectionId: props.collectionId, userId: props.collectionUserId, request: item });
            props.onClose();
        } catch {
            showRetryModal(() => onCreate(data));
        }
    };

    return (
        <Dialog open={props.open} onClose={props.onClose} maxWidth={'sm'} fullWidth>
            <DialogTitle>{props.cardId ? 'Изменение карточки' : 'Создание карточки'}</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form>
                        <FormField
                            label="Запомнить"
                            error={!!errors.frontText}
                            errorMessage={errors.frontText?.message}
                            {...register('frontText')}
                            autoFocus
                        />
                        <FormField
                            label="Подсказка (чтение)"
                            error={!!errors.promptText}
                            errorMessage={errors.promptText?.message}
                            {...register('promptText')}
                        />
                        <FormField
                            label="Значение"
                            error={!!errors.backText}
                            errorMessage={errors.backText?.message}
                            {...register('backText')}
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
                <Button onClick={handleSubmit(onCreate)} disabled={isLoading}>
                    {props.cardId ? 'Сохранить' : 'Создать'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export const CreateCardModal = withMutationResolver(
    useAddCardMutation,
    'Не удалось добавить карточку'
)(CreateCardModalContent);
