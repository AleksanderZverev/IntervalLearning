import { FC } from "react";
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
} from "@mui/material";
import { Form, FormField } from "../../../controls/Form/Form";
import { useCreateCourseMutation } from "../../../redux/courseApi";
import { withMutationResolver, WithMutationResolverProps } from "../../../hoc/withQueryResolver";
import { Controller, FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from "@hookform/resolvers/yup";
import * as yup from "yup";

interface CreateCourseForm {
    name: string;
    description: string;
}

interface CreateCourseModalProps extends WithMutationResolverProps<typeof useCreateCourseMutation> {
    isOpen: boolean;
    onClose: () => void;
}

const CreateCourseModalContent: FC<CreateCourseModalProps> = (
    {
        isOpen,
        mutationProps,
        onClose
    }: CreateCourseModalProps) => {

    const schema = yup
        .object({
            name: yup.string().required('Обязательное поле').max(255),
            description: yup.string().required('Обязательное поле')
        })
        .required();

    const formMethods = useForm<CreateCourseForm>({
        resolver: yupResolver(schema),
        defaultValues: {
            name: "",
            description: "",
        },
    });
    const { formState: { errors }, handleSubmit, register } = formMethods;
    const onCreate: SubmitHandler<CreateCourseForm> = async (data) => {

        try {
            await mutationProps.mutate({ name: data.name, description: data.description, isPrivate: false });
            onClose();
        } catch {
            mutationProps.showRetryModal(() => onCreate(data));
        }
    }
    return (
        <Dialog open={isOpen} fullWidth onClose={onClose} maxWidth={"sm"}>
            <DialogTitle>Создание курса</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form>
                        <Controller
                            name="name"
                            render={() => (
                                <FormField
                                    label="Название курса"
                                    error={!!errors.name}
                                    errorMessage={errors.name?.message}
                                    required
                                    {...register('name')}
                                />
                            )}
                        />
                        <Controller
                            name="description"
                            render={() => (
                                <FormField
                                    label="Описание курса"
                                    error={!!errors.description}
                                    errorMessage={errors.description?.message}
                                    required
                                    {...register('description')}
                                />
                            )}
                        />
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions>
                <Button variant="contained" onClick={handleSubmit(onCreate)}
                        disabled={mutationProps.isLoading}>
                    Создать
                </Button>
            </DialogActions>
        </Dialog>
    )
}

export const CreateCourseModal = withMutationResolver(
    useCreateCourseMutation,
    'Не удалось добавить курс'
)(CreateCourseModalContent);