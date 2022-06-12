import { Add, Done, People, ThumbDown, ThumbUp } from '@mui/icons-material';
import { CircularProgress, IconButton, TextField } from '@mui/material';
import classNames from 'classnames';
import { FC, useRef, useState } from 'react';
import { UserHelper } from '../../../helpers/UserHelper';
import { useSearchPublicCollectionQuery } from '../../../redux/collectionApi';
import styles from './styles.module.css';

interface CollectionsSearchProps {
    themeId: number;
}

export const CollectionsSearch: FC<CollectionsSearchProps> = ({ themeId }) => {
    const [searchName, setSearchName] = useState('');

    const timer = useRef<{ id: number | null }>({ id: null });

    const {
        data: foundCollections,
        isFetching,
        isError,
    } = useSearchPublicCollectionQuery({ searchName, page: 1, count: 30, themeId });

    return (
        <div style={{ marginLeft: 10, marginTop: 10 }}>
            <TextField
                fullWidth
                placeholder="Введите название"
                variant="standard"
                onChange={(e) => {
                    const newValue = e.target.value;

                    if (timer.current.id != null) {
                        window.clearTimeout(timer.current.id);
                    }

                    timer.current.id = window.setTimeout(() => setSearchName(newValue), 500);
                }}
                InputProps={{
                    endAdornment: isFetching ? <CircularProgress color="inherit" size={20} /> : undefined,
                }}
            />
            {!isError && (
                <ul>
                    {foundCollections?.map((c) => (
                        <li key={`${c.userId}-${c.id}`} className={styles.collection}>
                            <div className={styles.spaceContainer}>
                                <div className={styles.collectionTitle}>{c.title}</div>
                                {c.isAdded ? (
                                    <Done />
                                ) : (
                                    <IconButton>
                                        <Add />
                                    </IconButton>
                                )}
                            </div>

                            <div className={styles.spaceContainer}>
                                <div className={styles.collectionAuthor}>{UserHelper.getFullName(c.ownerUser)}</div>
                                <div className={styles.statistic}>
                                    <div className={styles.item}>
                                        <People fontSize="small" /> {c.publication?.subscribersCount}
                                    </div>
                                    <div className={styles.item}>
                                        <span className={classNames({ [styles.like]: c.isLiked })}>
                                            <ThumbUp fontSize="small" />{' '}
                                        </span>
                                        <span className={styles.like}>{c.publication?.likesCount}</span>
                                    </div>
                                    <div className={styles.item}>
                                        <span className={classNames({ [styles.dislike]: c.isDisliked })}>
                                            <ThumbDown fontSize="small" />{' '}
                                        </span>
                                        <span className={styles.dislike}>{c.publication?.dislikesCount}</span>
                                    </div>
                                </div>
                            </div>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};
