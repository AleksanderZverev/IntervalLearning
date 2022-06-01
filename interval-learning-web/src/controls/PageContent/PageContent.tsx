import { FC, PropsWithChildren } from 'react';

export const PageContent: FC<PropsWithChildren<{}>> = ({ children }) => {
    return <div>{children}</div>;
};
