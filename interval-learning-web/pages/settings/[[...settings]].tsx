import React, { FC } from 'react';
import { Route, Routes } from 'react-router-dom';
import { ThemesPage } from '../../src/pages/settings/ThemesPage/ThemesPage';
import { LanguagesPage } from '../../src/pages/settings/LanguagesPage/LanguagesPage';

const SettingsPageRouter: FC = () => {
    return (
        <Routes>
            <Route path="/settings/themes" element={<ThemesPage />} />
            <Route path="/settings/languages" element={<LanguagesPage />} />
        </Routes>
    );
};

export default SettingsPageRouter;
