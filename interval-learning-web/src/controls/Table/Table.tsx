import { FC, PropsWithChildren } from 'react';
import {
    Table as MuiTable,
    TableBody as MuiTableBody,
    TableCell as MuiTableCell,
    TableRow as MuiTableRow,
    TableHead as MuiTableHead,
    TableCellProps,
} from '@mui/material';
import styles from './styles.module.css';

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

export const TableRow: FC<PropsWithChildren<unknown>> = ({ children }) => {
    return <MuiTableRow>{children}</MuiTableRow>;
};

interface TableHeaderCell extends TableCellProps {}

export const TableHeaderCell: FC<PropsWithChildren<TableHeaderCell>> = ({ children, ...props }) => {
    return (
        <MuiTableCell {...props} sx={{ color: '#ADADAD' }}>
            {children}
        </MuiTableCell>
    );
};

export const TableCell: FC<PropsWithChildren<TableCellProps>> = ({ children, ...props }) => {
    return <MuiTableCell {...props}>{children}</MuiTableCell>;
};
