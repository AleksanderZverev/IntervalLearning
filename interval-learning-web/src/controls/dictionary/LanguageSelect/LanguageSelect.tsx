/* eslint-disable react/display-name */
import { MenuItem, Select, SelectProps } from '@mui/material';
import { FC, forwardRef } from 'react';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectLanguages } from '../../../redux/slices/languagesSlice';
import { Language } from '../../../types/Dictionary';

interface LanguageSelectProps extends SelectProps {}

export const LanguageSelect = forwardRef<typeof Select, SelectProps>(({ ...selectProps }, ref) => {
    var languages = useTypedSelector(selectLanguages);

    if (languages === undefined || languages.length === 0) {
        throw new Error();
    }

    return (
        <Select ref={ref} size="small" fullWidth {...selectProps}>
            {languages.map((l) => (
                <MenuItem key={l.id} value={l.id}>
                    {l.name}
                </MenuItem>
            ))}
        </Select>
    );
});
