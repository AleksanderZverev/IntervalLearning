import classNames from 'classnames';
import Head from 'next/head';
import React, { FC, useState } from 'react';
import { PageContainer } from '../../src/controls/PageContainer/PageContainer';
import { PageContent } from '../../src/controls/PageContent/PageContent';
import { withQueryResolver } from '../../src/hoc/withQueryResolver';
import useTypedSelector from '../../src/hooks/useTypedSelector';
import { CollectionsSearch } from '../../src/pages/store/collections/CollectionsSearch';
import { selectThemes } from '../../src/redux/slices/themeSlice';
import { useGetThemesQuery } from '../../src/redux/themeSlice';
import styles from './styles.module.css';

const StorePageRouter: FC = () => {
    const type = 'english';
    const themes = useTypedSelector(selectThemes);
    const [selectedThemeId, setSelectedThemeId] = useState<number>(themes[0].id);

    return (
        <>
            <Head>
                <title>🔍 Поиск</title>
            </Head>
            <PageContainer>
                <div className={styles.header}>
                    <div style={{ padding: 8, fontSize: 20, borderBottom: '2px solid #1976d2' }}>Коллекции</div>
                </div>
                <PageContent>
                    <div className={styles.contentContainer}>
                        <ul className={styles.themesList}>
                            {themes.map((t) => (
                                <li
                                    key={t.id}
                                    onClick={() => setSelectedThemeId(t.id)}
                                    className={classNames({ [styles.active]: selectedThemeId === t.id })}
                                >
                                    {t.name}
                                </li>
                            ))}
                        </ul>
                        <CollectionsSearch themeId={selectedThemeId} />
                    </div>
                </PageContent>
            </PageContainer>
        </>
    );
};

const WithThemeLoading = withQueryResolver(useGetThemesQuery)(StorePageRouter);

export default WithThemeLoading;
