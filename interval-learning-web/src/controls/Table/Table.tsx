import { FC, HTMLAttributes, PropsWithChildren } from 'react';
import {
    Table as MuiTable,
    TableBody as MuiTableBody,
    TableCell as MuiTableCell,
    TableRow as MuiTableRow,
    TableHead as MuiTableHead,
    TableCellProps as MuiTableCellProps,
} from '@mui/material';
import styles from './styles.module.css';
import { Link } from 'react-router-dom';

export const Table: FC<PropsWithChildren<unknown>> = ({ children }) => {
    return <MuiTable>{children}</MuiTable>;
};

interface TableHeadProps {
    hasBorder?: boolean;
}

export const TableHead: FC<PropsWithChildren<TableHeadProps>> = ({ children, hasBorder }) => {
    return (
        <MuiTableHead>
            <MuiTableRow
                sx={{
                    '& .MuiTableCell-head': {
                        border: hasBorder ? undefined : 'none',
                        paddingBottom: '8px',
                    },
                }}
            >
                {children}
            </MuiTableRow>
        </MuiTableHead>
    );
};

export const TableBody: FC<PropsWithChildren<unknown>> = ({ children }) => {
    return <MuiTableBody>{children}</MuiTableBody>;
};

interface TableRowProps extends HTMLAttributes<HTMLTableRowElement> {
    borderless?: boolean;
    hover?: boolean;
}

export const TableRow: FC<PropsWithChildren<TableRowProps>> = ({ children, borderless, hover, ...props }) => {
    return (
        <MuiTableRow
            hover={hover}
            sx={{
                '&.MuiTableRow-hover': {
                    cursor: 'pointer',
                },
                '& .MuiTableCell-body': {
                    border: borderless ? 'none' : undefined,
                },
            }}
            {...props}
        >
            {children}
        </MuiTableRow>
    );
};

interface TableHeaderCellProps extends MuiTableCellProps {}

export const TableHeaderCell: FC<PropsWithChildren<TableHeaderCellProps>> = ({ children, ...props }) => {
    return (
        <MuiTableCell {...props} style={{ color: '#ADADAD', fontSize: 14 }}>
            {children}
        </MuiTableCell>
    );
};

interface TableCellProps extends MuiTableCellProps {
    fontSize?: number;
}

export const TableCell: FC<PropsWithChildren<TableCellProps>> = ({ children, fontSize, ...props }) => {
    return (
        <MuiTableCell {...props} style={{ fontSize: fontSize ?? 20 }}>
            {children}
        </MuiTableCell>
    );
};
