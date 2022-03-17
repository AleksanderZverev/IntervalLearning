import { Button, Dialog, DialogActions, DialogContent, DialogTitle } from '@mui/material';
import { FC, useState } from 'react';
import { collectionLimits } from '../../globals/constants';
import { Theme, validatedDefault } from '../../types/global';
import { SelectTheme } from '../SelectTheme/SelectTheme';
import { ValidatedTextField } from '../ValidatedTextField/ValidatedTextField';

interface CreateCollectionModalProps {
    open: boolean;
    onClose: () => void;
}

const getDefaultForm = () => ({
    title: validatedDefault<string | null>(''),
    theme: validatedDefault<Theme | null>(null),
});

export const CreateCollectionModal: FC<CreateCollectionModalProps> = (props) => {
    const [form, setForm] = useState(getDefaultForm());

    const isValid = (): boolean => Object.values(form).some((v) => v.error);

    const onCreate = () => {
        if (!isValid()) {
            return;
        }
        console.log('ok');
    };

    console.log('form', form);
    return (
        <Dialog open={props.open} onClose={props.onClose} maxWidth={'sm'} fullWidth>
            <DialogTitle>Создание коллекции</DialogTitle>
            <DialogContent>
                <SelectTheme value={form.theme} onValueChange={(theme) => setForm({ ...form, theme })} notNullOrEmpty />
                <ValidatedTextField
                    value={form.title}
                    onValueChange={(v) => setForm({ ...form, title: v })}
                    label="Название"
                    notNullOrEmpty
                    maxLength={collectionLimits.titleMaxLength}
                />
            </DialogContent>
            <DialogActions>
                <Button onClick={onCreate}>Создать</Button>
            </DialogActions>
        </Dialog>
    );
};
