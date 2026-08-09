import { Button, TextField } from "@mui/material";
import { FC, useState } from "react";
import { LanguageSelect } from "../../src/controls/dictionary/LanguageSelect/LanguageSelect";
import { PageContainer } from "../../src/controls/PageContainer/PageContainer";
import { PageHeader } from "../../src/controls/PageHeader/PageHeader";
import { withQueryResolver } from "../../src/hoc/withQueryResolver";
import {
  AddTranslationsRequest,
  useAddTranslationsMutation,
  useGetLanguagesQuery,
} from "../../src/redux/api/dictionaryApi";
import * as yup from "yup";
import { useForm } from "react-hook-form";
import { yupResolver } from "@hookform/resolvers/yup";
import { Form, FormFiledLabel } from "../../src/controls/Form/Form";
import { ModalLoader } from "../../src/ModalLoader/ModalLoader";

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

const inputDataPlaceholder = `word_text1 pronunciation1 translation1_1,translation1_2,translation1_3
word_text2 pronunciation2 translation2_1,translation2_2,translation2_3
word_text3 pronunciation3 translation3_1,translation3_2,translation3_3
...
`;

interface DictionaryPageProps {}

const DictionaryPage: FC<DictionaryPageProps> = ({}) => {
  const [error, setError] = useState("");
  const [loadingTitle, setLoadingTitle] = useState("Загрузка");
  const [addTranslations, {}] = useAddTranslationsMutation();
  const [isLoading, setIsLoading] = useState(false);

  const formMethods = useForm<IForm>({
    resolver: yupResolver(schema),
    defaultValues: schema.getDefault(),
  });

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = formMethods;

  const onLoad = async (data: IForm) => {
    const textSplit = data.text.split("\n");

    let count = 0;
    let maxLines = 100;
    let totalCount = 0;
    let lines: string[] = [];
    const allErrors: string[] = [];

    setIsLoading(true);

    setLoadingTitle("Загружено " + totalCount + " из " + textSplit.length);

    for (const line of textSplit) {
      if (count >= maxLines) {
        totalCount += count;
        count = 0;

        const text = lines.join("\n");

        const request: AddTranslationsRequest = {
          languageId: data.languageId,
          translationLanguageId: data.translateLanguageId,
          text: text,
        };

        try {
          const errors = await addTranslations(request).unwrap();

          if (errors) {
            allErrors.push(errors);
          }
        } catch (e) {
          console.error(e);
        }

        setLoadingTitle("Загружено " + totalCount + " из " + textSplit.length);
        lines = [];
      }

      lines.push(line);
      count++;
    }

    if (allErrors.length > 0) {
      setError(allErrors.join("\n"));
    }

    setValue("text", "");
    setIsLoading(false);
  };
  return (
    <PageContainer>
      <PageHeader title="Загрзука данных в словарь" />
      <ModalLoader loading={isLoading} title={loadingTitle} />
      <div>
        <Button onClick={handleSubmit(onLoad)} variant="contained">
          Загрузить
        </Button>
        <Form>
          <FormFiledLabel label="Слова на языке">
            <LanguageSelect {...register("languageId")} />
            {errors.languageId && errors.languageId.message}
          </FormFiledLabel>
          <FormFiledLabel label="Язык перевода">
            <LanguageSelect {...register("translateLanguageId")} />
            {errors.translateLanguageId && errors.translateLanguageId.message}
          </FormFiledLabel>
          <TextField
            multiline
            minRows={5}
            {...register("text")}
            placeholder={inputDataPlaceholder}
          />
          {errors.text && errors.text.message}
        </Form>
        {error && <code style={{ whiteSpace: "pre-wrap" }}>{error}</code>}
      </div>
    </PageContainer>
  );
};

const WithLanguages = withQueryResolver(useGetLanguagesQuery)(DictionaryPage);

export default WithLanguages;
