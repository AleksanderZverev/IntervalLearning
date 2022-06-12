import { Edit } from '@mui/icons-material';
import { IconButton, TextField, Typography } from '@mui/material';
import { FC, ReactNode, useState } from 'react';
import styles from './styles.module.css';

interface PageHeaderProps {
    title: string;
    titleIcon?: ReactNode;
    subTitle?: string;
    editable?: boolean;
    onChange?: (newTitle: string) => void;
    subMenu?: ReactNode;
}

export const PageHeader: FC<PageHeaderProps> = ({ title, subTitle, subMenu, editable, titleIcon, onChange }) => {
    const [editMode, setEditMode] = useState(false);

    const setEdit = (value: boolean) => {
        if (!editable) {
            return;
        }
        setEditMode(value);
    };

    return (
        <div className={styles.container}>
            <div className={styles.innerContainer}>
                {editMode ? (
                    <TextField
                        value={title}
                        onChange={(e) => onChange && onChange(e.target.value)}
                        onBlur={() => setEdit(false)}
                        onKeyPress={(e) => e.key === 'Enter' && setEdit(false)}
                        variant={'standard'}
                        fullWidth
                        inputProps={{ style: { fontSize: 36, fontWeight: 'lighter', padding: 0 } }}
                        autoFocus
                    />
                ) : (
                    <div style={{ display: 'flex', alignItems: 'center', columnGap: 5 }}>
                        <Typography variant="h1" fontSize={36}>
                            <span onDoubleClick={() => setEdit(true)}>{title}</span>
                        </Typography>
                        {titleIcon}
                        {editable && (
                            <IconButton style={{ marginLeft: '6px' }} onClick={() => setEdit(true)}>
                                <Edit />
                            </IconButton>
                        )}
                    </div>
                )}
                {!editMode && subMenu}
            </div>
            {subTitle && <div className={styles.subTitle}>{subTitle}</div>}
            {!editMode && <div className={styles.endLine} />}
        </div>
    );
};
