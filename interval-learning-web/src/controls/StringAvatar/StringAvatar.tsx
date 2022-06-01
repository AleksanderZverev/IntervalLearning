import { Avatar, AvatarProps } from '@mui/material';
import { FC } from 'react';
import { StringHelper } from '../../helpers/StringHelper';

function getLetters(name: string): string {
    return `${name.split(' ')[0][0]}${name.split(' ')[1][0]}`;
}

interface StringAvatarProps extends AvatarProps {
    name: string;
    size?: number | string;
    fontSize?: number | string;
}

export const StringAvatar: FC<StringAvatarProps> = ({ name, size, fontSize, ...avatarProps }) => {
    return (
        <Avatar
            sx={{ bgcolor: StringHelper.stringToColor(name), width: size, height: size, fontSize: fontSize }}
            {...avatarProps}
        >
            {getLetters(name)}
        </Avatar>
    );
};
