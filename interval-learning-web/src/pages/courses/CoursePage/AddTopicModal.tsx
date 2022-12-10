import { FC } from "react";
import { Button, Dialog, DialogActions, DialogContent, DialogTitle } from "@mui/material";
import { Form, FormField } from "../../../controls/Form/Form";
import { Controller, FormProvider, SubmitHandler, useForm } from "react-hook-form";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { withMutationResolver, WithMutationResolverProps } from "../../../hoc/withQueryResolver";
import { useCreateTopicMutation } from "../../../redux/courseApi";

export interface AddTopicNodalProps extends WithMutationResolverProps<typeof useCreateTopicMutation> {
    courseId: string;
    isOpen: boolean;
    onClose: () => void;
}

interface AddTopicForm {
    name: string;
    theory: string;
}

export const AddTopicModalComponent: FC<AddTopicNodalProps> = (
    {
        mutationProps: { mutate, showRetryModal, isLoading },
        onClose,
        courseId,
        isOpen
    }) => {

    const schema = yup
        .object({
            name: yup.string().required('Обязательное поле').max(255)
        })
        .required();

    const formMethods = useForm<AddTopicForm>({
        resolver: yupResolver(schema),
        defaultValues: {
            name: "",
        },
    });
    const { formState: { errors }, handleSubmit, register } = formMethods;
    const onCreate: SubmitHandler<AddTopicForm> = async (data) => {
        try {
            await mutate({ name: data.name, courseId: courseId, theory: ""});
            onClose();
        } catch (e) {
            showRetryModal(() => onCreate(data));
        }
    }

    return (
        <Dialog open={isOpen} onClose={() => onClose()}>
            <DialogTitle>Добавить тему</DialogTitle>
            <DialogContent>
                <FormProvider {...formMethods}>
                    <Form>
                        <Controller
                            name="name"
                            render={() => (
                                <FormField
                                    label="Название темы"
                                    error={!!errors.name}
                                    errorMessage={errors.name?.message}
                                    required
                                    {...register('name')}
                                />
                            )}
                        />
                    </Form>
                </FormProvider>
            </DialogContent>
            <DialogActions>
                <Button variant="contained" onClick={handleSubmit(onCreate)}
                        disabled={isLoading}>
                    Создать
                </Button>
            </DialogActions>
        </Dialog>
    )
}

export const AddTopicModal = withMutationResolver(useCreateTopicMutation, 'Не удалось добавить тему')
(AddTopicModalComponent);