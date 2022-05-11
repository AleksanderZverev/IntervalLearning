import { Dialog, DialogTitle, DialogContent, DialogActions, Button, Checkbox, FormControlLabel } from '@mui/material';
import { FC, useState } from 'react';
import { LocalStorageHelper } from '../../helpers/localStorageHelper';

interface AssertionModalProps {
    title: string;
    message: string | null;
    assertTitle: string;
    cancelTitle?: string;
    onClose: () => void;
    forceOpen?: boolean;
    onAssert?: () => void;
    onCancel?: () => void;
    forbidShowingKey?: string;
}

export const AssertionModal: FC<AssertionModalProps> = ({
    title,
    message,
    cancelTitle,
    assertTitle,
    forceOpen,
    forbidShowingKey,
    ...props
}) => {
    const [forbidShowing, setForbidShowing] = useState(false);

    const onClose = () => {
        props.onClose();
    };

    const onCancel = () => {
        props.onCancel ? props.onCancel() : props.onClose();
    };

    const onAssert = () => {
        if (forbidShowing && forbidShowingKey) {
            LocalStorageHelper.setForbidShowing(forbidShowingKey);
        }

        props.onAssert ? props.onAssert() : props.onClose();
    };

    const isForbidden = forbidShowingKey ? LocalStorageHelper.hasForbidShowing(forbidShowingKey) : false;

    const forbidShowingControl = (
        <div>
            <FormControlLabel
                control={<Checkbox checked={forbidShowing} onChange={(e, v) => setForbidShowing(v)} />}
                label={'Не показывать больше'}
                labelPlacement="end"
            />
        </div>
    );

    const showForbidControl = Boolean(forbidShowingKey && !forceOpen);

    return (
        <Dialog open={forceOpen || !isForbidden} onClose={onClose} maxWidth={'sm'} fullWidth>
            <DialogTitle>{title}</DialogTitle>
            <DialogContent>{message}</DialogContent>
            <DialogActions>
                <div
                    style={{
                        display: 'flex',
                        flexDirection: 'column',
                        rowGap: 5,
                        alignItems: 'stretch',
                        width: '100%',
                        padding: 10,
                    }}
                >
                    {showForbidControl && cancelTitle && forbidShowingControl}

                    <div style={{ display: 'flex', justifyContent: 'space-between', width: '100%' }}>
                        {cancelTitle && (
                            <Button variant="outlined" onClick={onCancel}>
                                {cancelTitle}
                            </Button>
                        )}
                        {showForbidControl && !cancelTitle && forbidShowingControl}
                        {!showForbidControl && !cancelTitle && <div />}

                        <Button variant="contained" onClick={onAssert}>
                            {assertTitle}
                        </Button>
                    </div>
                </div>
            </DialogActions>
        </Dialog>
    );
};
