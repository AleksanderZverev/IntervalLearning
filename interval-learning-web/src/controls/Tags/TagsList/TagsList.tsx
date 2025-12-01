import { FC } from 'react';
import _ from 'lodash';
import styles from './styles.module.css';
import { Stack } from '@mui/material';
import { Tag } from '../TagView/Tag';

interface TagsListProps {
    cardUniqueId: string;
    tags: string[] | null;
}

export const TagsList: FC<TagsListProps> = ({ cardUniqueId, tags }) => {
    if (!tags || tags.length === 0) return null;

    return (
        <Stack direction={'row'} flexWrap={'wrap'} gap={'4px 8px'}>
            {tags.map((t, i) => (
                <Tag key={cardUniqueId + '-' + t + '-' + i}>{t}</Tag>
            ))}
        </Stack>
    );
};
