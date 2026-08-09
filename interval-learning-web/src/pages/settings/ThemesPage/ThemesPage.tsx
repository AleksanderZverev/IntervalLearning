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
import { useGetThemesQuery, useCreateThemeMutation, useUpdateThemeMutation, useDeleteThemeMutation, ThemeRequest } from '../../../redux/themeSlice';
import { selectThemes } from '../../../redux/slices/themeSlice';
import { Theme } from '../../../types/global';
import { AssertionModal } from '../../../controls/Modals/AssertionModal';

const pageTitle = 'Темы';

interface ThemeFormValues {
    name: string;
}

const schema = yup.object({ name: yup.string().max(100).required() }).required();

interface ThemeDialogProps {
    open: boolean;
    theme?: Theme;
    onClose: () => void;
}

const ThemeDialog: FC<ThemeDialogProps> = ({ open, theme, onClose }) => {
    const [createTheme, { isLoading: isCreating }] = useCreateThemeMutation();
    const [updateTheme, { isLoading: isUpdating }] = useUpdateThemeMutation();

    const formMethods = useForm<ThemeFormValues>({
        resolver: yupResolver(schema),
        defaultValues: theme ? { name: theme.name } : undefined,
    });

    const { handleSubmit, register, formState: { errors }, reset } = formMethods;

    const onSubmit: SubmitHandler<ThemeFormValues> = async (data) => {
        const request: ThemeRequest = { name: data.name };
        if (theme) {
            await updateTheme({ id: theme.id, data: request });
        } else {
            await createTheme(request);
        }
        reset();
        onClose();
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
            <DialogTitle sx={{ fontSize: 28 }}>{theme ? 'Изменить тему' : 'Создать тему'}</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form>
                        <FormField
                            label="Название"
                            error={!!errors.name}
                            errorMessage={errors.name?.message}
                            {...register('name')}
                        />
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions sx={{ px: 3, pb: 2 }}>
                <Button onClick={onClose}>Отмена</Button>
                <Button
                    variant="contained"
                    onClick={handleSubmit(onSubmit)}
                    disabled={isCreating || isUpdating}
                >
                    {theme ? 'Сохранить' : 'Создать'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

const ThemesPageContent: FC = () => {
    const themes = useTypedSelector(selectThemes);
    const [deleteTheme] = useDeleteThemeMutation();

    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingTheme, setEditingTheme] = useState<Theme | undefined>(undefined);
    const [deletingTheme, setDeletingTheme] = useState<Theme | undefined>(undefined);

    const openCreate = () => {
        setEditingTheme(undefined);
        setDialogOpen(true);
    };

    const openEdit = (theme: Theme) => {
        setEditingTheme(theme);
        setDialogOpen(true);
    };

    const onDelete = async () => {
        if (!deletingTheme) return;
        await deleteTheme(deletingTheme.id);
        setDeletingTheme(undefined);
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
            <Table>
                <TableHead>
                    <TableHeaderCell>Название</TableHeaderCell>
                    <TableHeaderCell width={90}></TableHeaderCell>
                </TableHead>
                <TableBody>
                    {themes.map((theme) => (
                        <TableRow key={theme.id} hover>
                            <TableCell>{theme.name}</TableCell>
                            <TableCell width={90} align="right">
                                <Stack direction="row" justifyContent="flex-end">
                                    <IconButton size="small" onClick={() => openEdit(theme)}>
                                        <Edit fontSize="small" />
                                    </IconButton>
                                    <IconButton size="small" color="error" onClick={() => setDeletingTheme(theme)}>
                                        <Delete fontSize="small" />
                                    </IconButton>
                                </Stack>
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
            {dialogOpen && (
                <ThemeDialog
                    open={dialogOpen}
                    theme={editingTheme}
                    onClose={() => setDialogOpen(false)}
                />
            )}
            {deletingTheme && (
                <AssertionModal
                    title={`Удалить тему «${deletingTheme.name}»?`}
                    message="Удалённую тему нельзя будет восстановить."
                    assertTitle="Удалить"
                    cancelTitle="Отмена"
                    onAssert={onDelete}
                    onClose={() => setDeletingTheme(undefined)}
                    onCancel={() => setDeletingTheme(undefined)}
                />
            )}
        </PageContainer>
    );
};

const ConnectedThemesPage = withQueryResolver(useGetThemesQuery)(ThemesPageContent);

export const ThemesPage: FC = () => (
    <>
        <Head>
            <title>{pageTitle}</title>
        </Head>
        <ConnectedThemesPage queryArg={undefined} />
    </>
);
