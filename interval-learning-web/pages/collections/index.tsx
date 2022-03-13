import { Container } from '@mui/material';
import { FC, useEffect, useLayoutEffect, useState } from 'react';
import { useGetCollectionsQuery } from '../../src/redux/collectionSlice';

const CollectionsPage: FC = () => {
    const { data: collections, isFetching, isError } = useGetCollectionsQuery();

    if (isFetching) return <div>LOADING...</div>;
    if (isError) return <div>ERROR...</div>;

    return (
        <div>
            <div>Collections page</div>
            <div>
                {collections?.map((c) => (
                    <div key={c.id}>{c.title}</div>
                ))}
            </div>
        </div>
    );
};

(CollectionsPage as any).auth = true;

export default CollectionsPage;
