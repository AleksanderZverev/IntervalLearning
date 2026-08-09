import { Add, Delete, Edit } from '@mui/icons-material';
import { Button, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, Stack } from '@mui/material';
import Head from 'next/head';
import { FC, useState } from 'react';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../controls/Table/Table';
import { Form, FormField } from '../../../controls/Form/Form';
import { withQueryResolver } from '../../../hoc/withQueryResolver';
import useTypedSelector from '../../../hooks/useTypedSelector';
import {
    useGetLanguagesQuery,
    useCreateLanguageMutation,
    useUpdateLanguageMutation,
    useDeleteLanguageMutation,
    LanguageRequest,
} from '../../../redux/api/dictionaryApi';
import { selectLanguages } from '../../../redux/slices/languagesSlice';
import { Language } from '../../../types/Dictionary';
import { AssertionModal } from '../../../controls/Modals/AssertionModal';

const pageTitle = 'Языки';

interface LanguageFormValues {
    name: string;
    nativeLanguageName: string;
    translationLink: string;
    translationLinkTitle: string;
}

const schema = yup
    .object({
        name: yup.string().max(50).required(),
        nativeLanguageName: yup.string().max(50).required(),
        translationLink: yup.string().optional(),
        translationLinkTitle: yup.string().max(50).optional(),
    })
    .required();

interface LanguageDialogProps {
    open: boolean;
    language?: Language;
    onClose: () => void;
}

const LanguageDialog: FC<LanguageDialogProps> = ({ open, language, onClose }) => {
    const [createLanguage, { isLoading: isCreating }] = useCreateLanguageMutation();
    const [updateLanguage, { isLoading: isUpdating }] = useUpdateLanguageMutation();

    const formMethods = useForm<LanguageFormValues>({
        resolver: yupResolver(schema),
        defaultValues: language
            ? {
                  name: language.name,
                  nativeLanguageName: language.nativeLanguageName,
                  translationLink: language.translationLink ?? '',
                  translationLinkTitle: language.translationLinkTitle ?? '',
              }
            : undefined,
    });

    const {
        handleSubmit,
        register,
        formState: { errors },
        reset,
    } = formMethods;

    const onSubmit: SubmitHandler<LanguageFormValues> = async (data) => {
        const request: LanguageRequest = {
            name: data.name,
            nativeLanguageName: data.nativeLanguageName,
            translationLink: data.translationLink || null,
            translationLinkTitle: data.translationLinkTitle || null,
        };
        if (language) {
            await updateLanguage({ id: language.id, data: request });
        } else {
            await createLanguage(request);
        }
        reset();
        onClose();
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
            <DialogTitle sx={{ fontSize: 28 }}>{language ? 'Изменить язык' : 'Создать язык'}</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form>
                        <FormField
                            label="Название (на английском)"
                            error={!!errors.name}
                            errorMessage={errors.name?.message}
                            {...register('name')}
                        />
                        <FormField
                            label="Название (родное)"
                            error={!!errors.nativeLanguageName}
                            errorMessage={errors.nativeLanguageName?.message}
                            {...register('nativeLanguageName')}
                        />
                        <FormField
                            label="Ссылка для перевода"
                            error={!!errors.translationLink}
                            errorMessage={errors.translationLink?.message}
                            {...register('translationLink')}
                        />
                        <FormField
                            label="Подпись ссылки"
                            error={!!errors.translationLinkTitle}
                            errorMessage={errors.translationLinkTitle?.message}
                            {...register('translationLinkTitle')}
                        />
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions sx={{ px: 3, pb: 2 }}>
                <Button onClick={onClose}>Отмена</Button>
                <Button variant="contained" onClick={handleSubmit(onSubmit)} disabled={isCreating || isUpdating}>
                    {language ? 'Сохранить' : 'Создать'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

const LanguagesPageContent: FC = () => {
    const languages = useTypedSelector(selectLanguages);
    const [deleteLanguage] = useDeleteLanguageMutation();

    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingLanguage, setEditingLanguage] = useState<Language | undefined>(undefined);
    const [deletingLanguage, setDeletingLanguage] = useState<Language | undefined>(undefined);

    const openCreate = () => {
        setEditingLanguage(undefined);
        setDialogOpen(true);
    };

    const openEdit = (language: Language) => {
        setEditingLanguage(language);
        setDialogOpen(true);
    };

    const onDelete = async () => {
        if (!deletingLanguage) return;
        await deleteLanguage(deletingLanguage.id);
        setDeletingLanguage(undefined);
    };

    return (
        <PageContainer>
            <PageHeader
                title={pageTitle}
                subMenu={
                    <Button variant="contained" endIcon={<Add />} onClick={openCreate}>
                        Создать
                    </Button>
                }
            />
            <div>
                <Table>
                    <TableHead>
                        <TableHeaderCell>Название</TableHeaderCell>
                        <TableHeaderCell>Родное название</TableHeaderCell>
                        <TableHeaderCell width={90}></TableHeaderCell>
                    </TableHead>
                    <TableBody>
                        {languages.map((language) => (
                            <TableRow key={language.id} hover>
                                <TableCell>{language.name}</TableCell>
                                <TableCell>{language.nativeLanguageName}</TableCell>
                                <TableCell width={90} align="right">
                                    <Stack direction="row" justifyContent="flex-end">
                                        <IconButton size="small" onClick={() => openEdit(language)}>
                                            <Edit fontSize="small" />
                                        </IconButton>
                                        <IconButton
                                            size="small"
                                            color="error"
                                            onClick={() => setDeletingLanguage(language)}
                                        >
                                            <Delete fontSize="small" />
                                        </IconButton>
                                    </Stack>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </div>
            {dialogOpen && (
                <LanguageDialog open={dialogOpen} language={editingLanguage} onClose={() => setDialogOpen(false)} />
            )}
            {deletingLanguage && (
                <AssertionModal
                    title={`Удалить язык «${deletingLanguage.name}»?`}
                    message="Удалённый язык нельзя будет восстановить."
                    assertTitle="Удалить"
                    cancelTitle="Отмена"
                    onAssert={onDelete}
                    onClose={() => setDeletingLanguage(undefined)}
                    onCancel={() => setDeletingLanguage(undefined)}
                />
            )}
        </PageContainer>
    );
};

const ConnectedLanguagesPage = withQueryResolver(useGetLanguagesQuery)(LanguagesPageContent);

export const LanguagesPage: FC = () => (
    <>
        <Head>
            <title>{pageTitle}</title>
        </Head>
        <ConnectedLanguagesPage queryArg={undefined} />
    </>
);
