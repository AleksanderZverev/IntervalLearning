import { FC, HTMLAttributes, PropsWithChildren } from 'react';
import {
    Table as MuiTable,
    TableBody as MuiTableBody,
    TableCell as MuiTableCell,
    TableRow as MuiTableRow,
    TableHead as MuiTableHead,
    TableCellProps,
} from '@mui/material';
import styles from './styles.module.css';
import { Link } from 'react-router-dom';

export const Table: FC<PropsWithChildren<unknown>> = ({ children }) => {
    return <MuiTable>{children}</MuiTable>;
};

export const TableHead: FC<PropsWithChildren<unknown>> = ({ children }) => {
    return (
        <MuiTableHead>
            <MuiTableRow>{children}</MuiTableRow>
        </MuiTableHead>
    );
};

export const TableBody: FC<PropsWithChildren<unknown>> = ({ children }) => {
    return <MuiTableBody>{children}</MuiTableBody>;
};

interface LinkTableRowProps {
    to: string;
}

export const LinkTableRow: FC<PropsWithChildren<LinkTableRowProps>> = ({ children, to }) => {
    return (
        <MuiTableRow component={Link} to={to}>
            {children}
        </MuiTableRow>
    );
};

interface TableRowProps extends HTMLAttributes<HTMLTableRowElement> {}

export const TableRow: FC<PropsWithChildren<TableRowProps>> = ({ children, ...props }) => {
    return <tr {...props}>{children}</tr>;
};

interface TableHeaderCell extends TableCellProps {}

export const TableHeaderCell: FC<PropsWithChildren<TableHeaderCell>> = ({ children, ...props }) => {
    return (
        <MuiTableCell {...props} style={{ color: '#ADADAD', fontSize: 16 }}>
            {children}
        </MuiTableCell>
    );
};

export const TableCell: FC<PropsWithChildren<TableCellProps>> = ({ children, ...props }) => {
    return (
        <MuiTableCell {...props} style={{ fontSize: 20 }}>
            {children}
        </MuiTableCell>
    );
};
