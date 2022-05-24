import { Button, TextField } from '@mui/material';
import { FC, useState } from 'react';
import { LanguageSelect } from '../../src/controls/dictionary/LanguageSelect/LanguageSelect';
import { CreateScheduleModal } from '../../src/controls/Modals/CreateScheduleModal';
import { PageContainer } from '../../src/controls/PageContainer/PageContainer';
import { PageHeader } from '../../src/controls/PageHeader/PageHeader';
import { withMutationResolver, WithMutationResolverProps, withQueryResolver } from '../../src/hoc/withQueryResolver';
import useTypedSelector from '../../src/hooks/useTypedSelector';
import {
    AddTranslationsRequest,
    useAddTranslationsMutation,
    useGetLanguagesQuery,
} from '../../src/redux/api/dictionaryApi';
import { selectSchedules } from '../../src/redux/slices/scheduleSlice';
import * as yup from 'yup';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Form, FormFiledLabel } from '../../src/controls/Form/Form';

interface IForm {
    text: string;
    languageId: string;
    translateLanguageId: string;
}

const schema = yup
    .object({
        text: yup.string().required(),
        languageId: yup.string().required(),
        translateLanguageId: yup.string().required(),
    })
    .required();

interface DictionaryPageProps extends WithMutationResolverProps<typeof useAddTranslationsMutation> {}

const DictionaryPage: FC<DictionaryPageProps> = ({ mutationProps: { mutate: addTranslations, showRetryModal } }) => {
    const [error, setError] = useState('');
    const formMethods = useForm<IForm>({ resolver: yupResolver(schema), defaultValues: schema.getDefault() });
    const {
        register,
        handleSubmit,
        formState: { errors },
    } = formMethods;

    const onLoad = async (data: IForm) => {
        console.log(data);

        const request: AddTranslationsRequest = {
            languageId: data.languageId,
            translationLanguageId: data.translateLanguageId,
            text: data.text,
        };

        try {
            const errors = await addTranslations(request);

            if (errors) {
                setError(errors);
            }
        } catch {
            showRetryModal(() => onLoad(data));
        }
    };
    return (
        <PageContainer>
            <PageHeader title="Словарь" />
            <div>
                <Button onClick={handleSubmit(onLoad)} variant="contained">
                    Add
                </Button>
                <Form>
                    <FormFiledLabel label="Слова на языке">
                        <LanguageSelect {...register('languageId')} />
                        {errors.languageId && errors.languageId.message}
                    </FormFiledLabel>
                    <FormFiledLabel label="Язык перевода">
                        <LanguageSelect {...register('translateLanguageId')} />
                        {errors.translateLanguageId && errors.translateLanguageId.message}
                    </FormFiledLabel>
                    <TextField multiline minRows={5} {...register('text')} />
                    {errors.text && errors.text.message}
                </Form>
                {error && <code style={{ whiteSpace: 'pre-wrap' }}>{error}</code>}
            </div>
        </PageContainer>
    );
};

const WithLanguages = withQueryResolver(useGetLanguagesQuery)(DictionaryPage);
const ConnectedMutation = withMutationResolver(useAddTranslationsMutation, 'Error')(WithLanguages);

export default ConnectedMutation;
