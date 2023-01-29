import { FC } from "react";
import { Button, Dialog, DialogActions, DialogContent, DialogTitle } from "@mui/material";
import { Form, FormField } from "../../../controls/Form/Form";
import { Controller, FormProvider, SubmitHandler, useForm } from "react-hook-form";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { withMutationResolver, WithMutationResolverProps } from "../../../hoc/withQueryResolver";
import { useCreateTopicCollectionMutation } from "../../../redux/courseApi";

export interface ModalProps extends WithMutationResolverProps<typeof useCreateTopicCollectionMutation> {
    isOpen: boolean;
    topicId: string;
    courseId: string;
    onClose: () => void;
}

export interface AddCollectionForm {
    name: string;
}

export const AddTopicCollectionModalContent: FC<ModalProps> =
    ({
         isOpen,
         courseId,
         topicId,
         onClose,
         mutationProps: { mutate, showRetryModal, isLoading }
     }) => {
        const schema = yup
            .object({
                name: yup.string().required('Обязательное поле').max(255)
            })
            .required();

        const formMethods = useForm<AddCollectionForm>({
            resolver: yupResolver(schema),
            defaultValues: {
                name: "",
            },
        });
        const { formState: { errors }, handleSubmit, register } = formMethods;
        const onCreate: SubmitHandler<AddCollectionForm> = async (data) => {
            try {
                await mutate({ name: data.name, courseId: courseId, topicId: topicId });
                onClose();
            } catch (e) {
                showRetryModal(() => onCreate(data));
            }
        }

        return (
            <Dialog open={isOpen} onClose={onClose}>
                <DialogTitle>Создать коллекцию</DialogTitle>
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

export const AddTopicCollectionModal = withMutationResolver(useCreateTopicCollectionMutation,
    'Не удалось создать коллекцию')(AddTopicCollectionModalContent)