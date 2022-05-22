import { yupResolver } from '@hookform/resolvers/yup';
import { Add, ArrowForwardIos, Info } from '@mui/icons-material';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    InputAdornment,
    Link,
    IconButton,
    Autocomplete,
} from '@mui/material';
import { FC } from 'react';
import { Controller, FormProvider, SubmitHandler, useFieldArray, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { withMutationResolver, WithMutationResolverProps } from '../../hoc/withQueryResolver';
import useTypedSelector from '../../hooks/useTypedSelector';
import { useLazyGetWordTranslationsQuery } from '../../redux/api/dictionaryApi';
import { CreateCardItem, useAddCardMutation } from '../../redux/cardsApi';
import { selectCardById } from '../../redux/slices/cardsSlice';
import { Card } from '../../types/Collection';
import { AsyncAutocomplete } from '../AsyncAutocomplete/AsyncAutocomplete';
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

interface CreateCardModalProps extends WithMutationResolverProps<typeof useAddCardMutation> {
    collectionId: string;
    collectionUserId: string;
    open: boolean;
    onClose: () => void;
    onAdded?: () => void;
    cardId?: string;
    defaultFrontText?: string;
    defaultPromptText?: string;
    defaultBackText?: string;
}

const CreateCardModalContent: FC<CreateCardModalProps> = ({
    mutationProps: { mutate: addCard, showRetryModal, isLoading },
    defaultFrontText,
    defaultPromptText,
    defaultBackText,
    ...props
}) => {
    const card = useTypedSelector((state) =>
        selectCardById(state, props.collectionUserId, props.collectionId, props.cardId)
    );

    const formMethods = useForm<CardForm>({
        resolver: yupResolver(schema),
        defaultValues: card
            ? getDefaultValues(card)
            : {
                  frontText: defaultFrontText,
                  promptText: defaultPromptText,
                  backText: defaultBackText,
                  examples: [{ value: '' }],
              },
    });

    const {
        register,
        handleSubmit,
        control,
        getValues,
        formState: { errors },
        watch,
    } = formMethods;

    const { fields, append } = useFieldArray({ control, name: 'examples' });

    const [getTranslation, {}] = useLazyGetWordTranslationsQuery();

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
            props.onAdded ? props.onAdded() : props.onClose();
        } catch {
            showRetryModal(() => onCreate(data));
        }
    };

    const frontTextValue = watch('frontText');

    return (
        <Dialog open={props.open} onClose={props.onClose} maxWidth={'sm'} fullWidth>
            <DialogTitle>{props.cardId ? 'Изменение карточки' : 'Создание карточки'}</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form>
                        <FormField
                            label="Запомнить (слово)"
                            error={!!errors.frontText}
                            errorMessage={errors.frontText?.message}
                            {...register('frontText')}
                            autoFocus
                            required
                            InputProps={{
                                endAdornment: (
                                    <InputAdornment position="end">
                                        <Link
                                            href={`https://wooordhunt.ru/word/${frontTextValue}`}
                                            color={'primary'}
                                            target="_blank"
                                            rel="noreferrer"
                                        >
                                            wooordhunt
                                        </Link>
                                    </InputAdornment>
                                ),
                            }}
                        />
                        <FormField
                            label="Подсказка (чтение)"
                            error={!!errors.promptText}
                            errorMessage={errors.promptText?.message}
                            {...register('promptText')}
                        />
                        <Controller
                            name="backText"
                            render={({ field: { value, ...field } }) => (
                                <AsyncAutocomplete
                                    label="Значение (перевод)"
                                    error={!!errors.backText}
                                    errorMessage={errors.backText?.message}
                                    required
                                    onChange={(e: string) => {
                                        field.onChange(e);
                                    }}
                                    value={value ?? ''}
                                    onFocus={async () => {
                                        try {
                                            const translations = await getTranslation(
                                                { word: frontTextValue },
                                                true
                                            ).unwrap();
                                            return translations.map((t) => ({
                                                label: t.translation,
                                                id: t.id,
                                            }));
                                        } catch {
                                            return [];
                                        }
                                    }}
                                />
                            )}
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
                        <div style={{ alignSelf: 'center' }}>
                            <IconButton onClick={onAddExample}>
                                <Add />
                            </IconButton>
                        </div>
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions style={{ padding: '20px', paddingTop: 0 }}>
                <Button variant="contained" onClick={handleSubmit(onCreate)} disabled={isLoading}>
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
