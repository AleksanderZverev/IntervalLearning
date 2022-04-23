import { Dialog, DialogTitle, DialogContent, DialogActions, Button } from '@mui/material';
import { FC } from 'react';

interface AssertionModalProps {
    title: string;
    message: string;
    open: boolean;
    cancelTitle?: string;
    assertTitle: string;
    onClose: () => void;
    onAssert: () => void;
    onCancel?: () => void;
}

export const AssertionModal: FC<AssertionModalProps> = ({
    title,
    message,
    cancelTitle,
    assertTitle,
    open,
    onClose,
    onAssert,
    onCancel,
}) => {
    return (
        <Dialog open={open} onClose={onClose} maxWidth={'sm'} fullWidth>
            <DialogTitle>{title}</DialogTitle>
            <DialogContent>{message}</DialogContent>
            <DialogActions>
                <div style={{ display: 'flex', justifyContent: 'space-between', width: '100%', padding: 10 }}>
                    {cancelTitle ? (
                        <Button variant="outlined" onClick={() => (onCancel ? onCancel() : onClose())}>
                            {cancelTitle}
                        </Button>
                    ) : (
                        <div />
                    )}
                    <Button variant="contained" onClick={onAssert}>
                        {assertTitle}
                    </Button>
                </div>
            </DialogActions>
        </Dialog>
    );
};
