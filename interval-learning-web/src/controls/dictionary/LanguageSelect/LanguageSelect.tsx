/* eslint-disable react/display-name */
import { MenuItem, Select, SelectProps, Stack } from '@mui/material';
import { FC, forwardRef } from 'react';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectLanguages } from '../../../redux/slices/languagesSlice';

interface LanguageSelectProps extends SelectProps {
    error?: boolean;
    errorMessage?: string;
}

export const LanguageSelect = forwardRef<typeof Select, LanguageSelectProps>(
    ({ error, errorMessage, ...selectProps }, ref) => {
        var languages = useTypedSelector(selectLanguages);

        return (
            <Stack direction={'column'} rowGap={'5px'}>
                <Select error={error} ref={ref} size="small" fullWidth {...selectProps}>
                    {languages?.map((l) => (
                        <MenuItem key={l.id} value={l.id}>
                            {l.name}
                        </MenuItem>
                    ))}
                </Select>
                <div style={{ color: 'red', height: '20px', fontSize: '14px', paddingLeft: '10px' }}>
                    {errorMessage}
                </div>
            </Stack>
        );
    }
);
