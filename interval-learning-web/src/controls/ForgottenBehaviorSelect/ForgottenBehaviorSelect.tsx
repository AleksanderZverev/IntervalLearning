import { MenuItem, Select, SelectProps } from '@mui/material';
import { FC } from 'react';
import { useFormContext } from 'react-hook-form';
import { ForgottenBehavior } from '../../types/schedule';

interface ForgottenBehaviorSelectProps extends SelectProps {
    registerName: string;
}

export const ForgottenBehaviorSelect: FC<ForgottenBehaviorSelectProps> = ({ registerName, ...props }) => {
    const { register } = useFormContext();

    return (
        <Select {...props} sx={{ width: 275 }} size="small" {...register(registerName)}>
            <MenuItem value={ForgottenBehavior.MoveToNextStep}>Перейти на следующий этап</MenuItem>
            <MenuItem value={ForgottenBehavior.MoveToPreviousStep}>Перейти на предыдущий этап</MenuItem>
            <MenuItem value={ForgottenBehavior.StartFromFirstStep}>Перейти на первый этап</MenuItem>
            <MenuItem value={ForgottenBehavior.StayOnCurrentStep}>Остаться на текущем этапе</MenuItem>
        </Select>
    );
};
