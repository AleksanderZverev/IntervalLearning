import { yupResolver } from '@hookform/resolvers/yup';
import { Add, ArrowForwardIos, Remove } from '@mui/icons-material';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    InputAdornment,
    Link,
    IconButton,
    Portal,
    TextField,
} from '@mui/material';
import { FC, useLayoutEffect, useRef, useState } from 'react';
import { Controller, FormProvider, SubmitHandler, useFieldArray, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { withMutationResolver, WithMutationResolverProps } from '../../hoc/withQueryResolver';
import useTypedSelector from '../../hooks/useTypedSelector';
import { useLazyGetWordTranslationsQuery, useLazySearchWordsQuery } from '../../redux/api/dictionaryApi';
import { CreateCardItem, useAddCardMutation, useLazyGetCardQuery } from '../../redux/cardsApi';
import { selectCardById } from '../../redux/slices/cardsSlice';
import { selectCollectionById } from '../../redux/slices/collectionsSlice';
import { selectLanguageById } from '../../redux/slices/languagesSlice';
import { selectTheme } from '../../redux/slices/themeSlice';
import { Card } from '../../types/Collection';
import { AsyncAutocomplete } from '../AsyncAutocomplete/AsyncAutocomplete';
import { Form, FormFiledLabel, IconFormField, TextAreaFormField } from '../Form/Form';
import { AssertionModal } from './AssertionModal';

interface Example {
    value: string;
}

interface Tag {
    value: string;
}

interface CardForm {
    frontText: string;
    promptText: string | null;
    backText: string;
    description: string | null;
    examples: Example[];
    tags: Tag[];
}

const exampleSchema = yup.object({
    value: yup.string().max(255),
});

const tagSchema = yup.object({
    value: yup.string().max(100),
});

const schema = yup
    .object({
        frontText: yup.string().required('Обязательное поле').max(255),
        backText: yup.string().required('Обязательное поле').max(255),
        description: yup.string().max(500).nullable(),
        examples: yup.array().of(exampleSchema),
        tags: yup.array().of(tagSchema),
    })
    .required();

function getDefaultValues(card: Card): CardForm {
    const examples = card.examples?.map((e) => ({ value: e })) ?? [];
    examples.push({ value: '' });

    const tags = card.tags?.map((e) => ({ value: e })) ?? [];
    tags.push({ value: '' });

    const cardForm: CardForm = {
        frontText: card.frontSideText,
        promptText: card.promptText,
        backText: card.backSideText,
        description: card.description,
        examples,
        tags,
    };

    return cardForm;
}

function CheckIfCardAlreadyChanged(oldCard: Card, newCard?: Card | null): boolean | null {
    if (!newCard) {
        return false;
    }

    return !Boolean(
        oldCard.backSideText === newCard.backSideText &&
            oldCard.frontSideText === newCard.frontSideText &&
            oldCard.promptText === newCard.promptText &&
            oldCard.description === newCard.description
    );
}

interface ReadOnlyState {
    cardId: string | undefined;
    collectionId: string;
    collectionUserId: string;
}

interface CreateCardModalProps extends WithMutationResolverProps<typeof useAddCardMutation> {
    collectionId: string;
    collectionUserId: string;
    open: boolean;
    onClose: () => void;
    onAdded?: (card: Card) => void;
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
    const {
        current: { cardId, collectionId, collectionUserId },
    } = useRef<ReadOnlyState>({
        cardId: props.cardId,
        collectionId: props.collectionId,
        collectionUserId: props.collectionUserId,
    });

    const collection = useTypedSelector((state) => selectCollectionById(state, collectionUserId, collectionId));

    const theme = useTypedSelector((state) => (collection ? selectTheme(state, collection.themeId) : undefined));

    const language = useTypedSelector((state) =>
        theme?.languageId ? selectLanguageById(state, theme.languageId) : undefined
    );

    const card = useTypedSelector((state) => selectCardById(state, collectionUserId, collectionId, cardId));

    const formMethods = useForm<CardForm>({
        resolver: yupResolver(schema),
        defaultValues: card
            ? getDefaultValues(card)
            : {
                  frontText: defaultFrontText,
                  promptText: defaultPromptText,
                  backText: defaultBackText,
                  examples: [{ value: '' }],
                  tags: [{ value: '' }],
              },
    });

    const {
        register,
        handleSubmit,
        control,
        getValues,
        setError,
        clearErrors,
        formState: { errors, dirtyFields },
        watch,
        trigger,
    } = formMethods;

    const { fields: exampleFields, append: appendExample } = useFieldArray({ control, name: 'examples' });
    const { fields: tagsFields, append: appendTag, remove: removeTag } = useFieldArray({ control, name: 'tags' });

    const [showCardStateChangedModal, setShowCardStateChangedModal] = useState(false);
    const [showSaveChangesModal, setSaveChangesModal] = useState(false);

    const [getTranslation, {}] = useLazyGetWordTranslationsQuery();
    const [searchWords, {}] = useLazySearchWordsQuery();
    const [getCard, {}] = useLazyGetCardQuery();

    const hasDirtyFields = Object.values(dirtyFields).filter(Boolean).length > 0;

    const onClose = (fromModal: boolean) => {
        if (!fromModal && hasDirtyFields) {
            setSaveChangesModal(true);
            return;
        }

        props.onClose();
    };

    const onAddExample = () => {
        const currentState = getValues();
        if (currentState.examples.every((e) => Boolean(e.value))) {
            appendExample({ value: '' });
        }
    };

    const onAddTag = () => {
        appendTag({ value: '' });
    };

    const onRemoveTag = (index: number) => {
        removeTag(index);
    };

    const onCreate: SubmitHandler<CardForm> = async (data) => {
        const item: CreateCardItem = {
            cardId: cardId,
            frontText: data.frontText,
            promptText: data.promptText,
            backText: data.backText,
            description: data.description,
            examples: data.examples.filter((e) => Boolean(e.value.trim())).map((e) => e.value),
            tags: data.tags.filter((e) => Boolean(e.value.trim())).map((e) => e.value),
        };

        if (card) {
            try {
                const currentCardState = await getCard({
                    userId: collectionUserId,
                    collectionId: collectionId,
                    request: {
                        cardId: card.id,
                    },
                }).unwrap();

                const isChanged = CheckIfCardAlreadyChanged(card, currentCardState);

                if (isChanged) {
                    setShowCardStateChangedModal(true);
                    return;
                }
            } catch {}
        }

        try {
            const card = await addCard({ collectionId: collectionId, userId: collectionUserId, request: item });
            props.onAdded ? props.onAdded(card) : props.onClose();
        } catch {
            showRetryModal(() => onCreate(data));
        }
    };

    return (
        <Dialog open={props.open} onClose={() => onClose(false)} maxWidth={'sm'} fullWidth>
            <DialogTitle>{cardId ? 'Изменение карточки' : 'Создание карточки'}</DialogTitle>
            <DialogContent>
                <Portal>
                    {showCardStateChangedModal && (
                        <AssertionModal
                            title="Невозможно обновить карточку"
                            message="Карточка изменилась за время просмотра"
                            onClose={() => {
                                setShowCardStateChangedModal(false);
                                props.onClose();
                            }}
                            assertTitle="OK"
                        />
                    )}
                    {showSaveChangesModal && (
                        <AssertionModal
                            title="Несохраненные данные"
                            message="Некоторые поля были изменены. Сохранить изменения?"
                            assertTitle="Сохранить"
                            onAssert={() => {
                                setSaveChangesModal(false);
                                handleSubmit(onCreate)();
                            }}
                            cancelTitle="Сбросить"
                            onClose={() => onClose(true)}
                        />
                    )}
                </Portal>
                <FormProvider {...formMethods}>
                    <Form>
                        <Controller
                            name="frontText"
                            render={({ field: { value, ...field } }) => (
                                <AsyncAutocomplete
                                    label="Запомнить (слово)"
                                    error={!!errors.frontText}
                                    errorMessage={errors.frontText?.message}
                                    required
                                    value={value ?? ''}
                                    onChange={(e) => {
                                        if (errors.frontText && errors.frontText.type === 'moveToTranslation') {
                                            clearErrors('frontText');
                                        }

                                        field.onChange(e);
                                    }}
                                    onFocus={async () => {
                                        const promptTextValue = watch('promptText')?.trim();

                                        if (!promptTextValue) {
                                            return [];
                                        }

                                        try {
                                            const words = await searchWords(
                                                { word: null, pronunciation: promptTextValue },
                                                true
                                            ).unwrap();

                                            if (words.length === 1 && words[0].word === watch('frontText')?.trim()) {
                                                return [];
                                            }

                                            return words
                                                .filter((w) => Boolean(w.pronunciation))
                                                .map((w) => ({
                                                    label: w.word ?? '',
                                                    id: w.id,
                                                }));
                                        } catch {
                                            return [];
                                        }
                                    }}
                                    textFieldProps={{
                                        autoFocus: !Boolean(defaultFrontText),
                                        InputProps: {
                                            endAdornment: language &&
                                                language.translationLink &&
                                                language.translationLinkTitle && (
                                                    <InputAdornment position="end">
                                                        <Link
                                                            href={language.translationLink.replace(
                                                                '[word]',
                                                                watch('frontText')
                                                            )}
                                                            color={'primary'}
                                                            target="_blank"
                                                            rel="noreferrer"
                                                            onClick={(e) => {
                                                                e.preventDefault();

                                                                const frontTextValue = watch('frontText');

                                                                if (frontTextValue && language.translationLink) {
                                                                    window.open(
                                                                        language.translationLink.replace(
                                                                            '[word]',
                                                                            frontTextValue
                                                                        ),
                                                                        '_blank',
                                                                        'noreferrer'
                                                                    );
                                                                    return;
                                                                }

                                                                setError('frontText', {
                                                                    message: 'Для перехода введите значение',
                                                                    type: 'moveToTranslation',
                                                                });
                                                            }}
                                                        >
                                                            {language.translationLinkTitle}
                                                        </Link>
                                                    </InputAdornment>
                                                ),
                                        },
                                    }}
                                />
                            )}
                        />
                        <Controller
                            name="promptText"
                            render={({ field: { value, ...field } }) => (
                                <AsyncAutocomplete
                                    label="Подсказка (чтение)"
                                    error={!!errors.promptText}
                                    errorMessage={errors.promptText?.message}
                                    onChange={field.onChange}
                                    value={value ?? ''}
                                    onFocus={async () => {
                                        const frontTextValue = watch('frontText')?.trim();

                                        if (!frontTextValue) {
                                            return [];
                                        }

                                        try {
                                            const words = await searchWords(
                                                { word: frontTextValue, pronunciation: null },
                                                true
                                            ).unwrap();

                                            if (
                                                words.length === 1 &&
                                                words[0].pronunciation === watch('promptText')?.trim()
                                            ) {
                                                return [];
                                            }

                                            return words
                                                .filter((w) => Boolean(w.pronunciation))
                                                .map((w) => ({
                                                    label: w.pronunciation ?? '',
                                                    id: w.id,
                                                }));
                                        } catch {
                                            return [];
                                        }
                                    }}
                                />
                            )}
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
                                        const frontTextValue = watch('frontText')?.trim();

                                        if (!frontTextValue) {
                                            return [];
                                        }

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
                        <FormFiledLabel label="Теги" />
                        <div
                            style={{
                                display: 'flex',
                                flexDirection: 'column',
                                gap: '8px',
                                flexWrap: 'wrap',
                                alignItems: 'flex-start',
                            }}
                        >
                            {tagsFields.map((tagField, i) => {
                                const errorMessage = errors.tags?.at(i)?.value?.message;
                                return (
                                    <div
                                        key={tagField.id}
                                        style={{
                                            display: 'flex',
                                            flexDirection: 'row',
                                            columnGap: '4px',
                                            alignItems: 'center',
                                            border: '1px solid #05c953',
                                            borderRadius: '24px',
                                            padding: '4px',
                                        }}
                                    >
                                        <TextField
                                            key={tagField.id}
                                            {...register(`tags.${i}.value`)}
                                            variant="standard"
                                            autoComplete="off"
                                            sx={{ marginLeft: '12px' }}
                                            inputProps={{
                                                style: {
                                                    minWidth: '180px',
                                                    width: '0',
                                                },
                                            }}
                                        />
                                        <IconButton onClick={() => onRemoveTag(i)}>
                                            <Remove htmlColor="#dc5d5d" />
                                        </IconButton>
                                        {errorMessage && <span>{errorMessage}</span>}
                                    </div>
                                );
                            })}
                        </div>
                        <div style={{ margin: '4px 0' }}>
                            <IconButton onClick={() => onAddTag()}>
                                <Add />
                            </IconButton>
                        </div>
                        <TextAreaFormField
                            label="Описание"
                            error={!!errors.description}
                            errorMessage={errors.description?.message}
                            {...register('description')}
                        />
                        <FormFiledLabel label="Примеры" />
                        <div style={{ display: 'flex', flexDirection: 'column' }}>
                            {exampleFields.map((f, i) => {
                                return (
                                    <div key={f.id}>
                                        <IconFormField
                                            label=""
                                            multiline
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
                    {cardId ? 'Сохранить' : 'Создать'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export const CreateCardModal = withMutationResolver(
    useAddCardMutation,
    'Не удалось добавить карточку'
)(CreateCardModalContent);
