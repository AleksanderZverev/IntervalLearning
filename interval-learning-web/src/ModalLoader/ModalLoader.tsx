import { Modal, Portal } from '@mui/material';
import { FC } from 'react';
import { CenterContainer } from '../controls/CenterContainer/CenterContainer';
import { Loader } from '../controls/Loader/Loader';

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
                        <Loader title={title} />
                    </CenterContainer>
                </Modal>
            )}
        </Portal>
    );
};
