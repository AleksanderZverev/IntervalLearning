import { CircularProgress, Modal, Portal, Stack } from '@mui/material';
import { blue } from '@mui/material/colors';
import { FC } from 'react';
import { CenterContainer } from '../controls/CenterContainer/CenterContainer';

interface ModalLoaderProps {
    loading: boolean;
    title?: string;
}

export const ModalLoader: FC<ModalLoaderProps> = ({ loading, title }) => {
    return (
        <Portal>
            {loading && (
                <Modal open>
                    <CenterContainer>
                        <Stack direction={'column'}>
                            <CircularProgress size={70} sx={{ color: blue[500] }} />
                            <label style={{ color: 'white' }}>{title || 'Загрузка'}</label>
                        </Stack>
                    </CenterContainer>
                </Modal>
            )}
        </Portal>
    );
};
