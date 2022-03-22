import { MenuItem, Select, SelectProps } from '@mui/material';
import { FC } from 'react';
import { ForgottenBehavior } from '../../types/schedule';

interface ForgottenBehaviorSelectProps extends SelectProps {}

export const ForgottenBehaviorSelect: FC<ForgottenBehaviorSelectProps> = ({ ...props }) => {
    return (
        <Select {...props} sx={{ width: 275 }} size="small" value={ForgottenBehavior.MoveToNextStep}>
            <MenuItem value={ForgottenBehavior.MoveToNextStep}>Перейти на следующий этап</MenuItem>
            <MenuItem value={ForgottenBehavior.MoveToPreviousStep}>Перейти на предыдущий этап</MenuItem>
            <MenuItem value={ForgottenBehavior.StartFromFirstStep}>Перейти на первый этап</MenuItem>
            <MenuItem value={ForgottenBehavior.StayOnCurrentStep}>Остаться на текущем этапе</MenuItem>
        </Select>
    );
};
