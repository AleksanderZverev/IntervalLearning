import { Dialog, DialogTitle, DialogContent, DialogActions, Button } from '@mui/material';
import { FC } from 'react';

interface ErrorModalProps {
    errorMessage: string;
    open: boolean;
    onClose: () => void;
    onRetry: () => void;
}

export const ErrorModal: FC<ErrorModalProps> = ({ errorMessage, open, onClose, onRetry }) => {
    return (
        <Dialog open={open} onClose={onClose} maxWidth={'sm'} fullWidth>
            <DialogTitle>Ошибка при отправке данных</DialogTitle>
            <DialogContent>{errorMessage}</DialogContent>
            <DialogActions>
                <div style={{ display: 'flex', justifyContent: 'space-between', width: '100%', padding: 10 }}>
                    <Button variant="outlined" onClick={onClose}>
                        Отмена
                    </Button>
                    <Button variant="contained" onClick={onRetry}>
                        Попробовать еще раз
                    </Button>
                </div>
            </DialogActions>
        </Dialog>
    );
};
