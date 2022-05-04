import { Autocomplete } from '@mui/material';
import { green } from '@mui/material/colors';
import { forwardRef } from 'react';
import { Controller } from 'react-hook-form';
import { useSelector } from 'react-redux';
import { selectSchedules } from '../../redux/slices/scheduleSlice';
import { Schedule } from '../../types/schedule';
import { FormField, FormFieldProps } from '../Form/Form';

interface SelectScheduleProps {
    scheduleUserId: string | undefined;
    scheduleId: string | undefined;
    onChange: (newItem: Schedule | undefined) => void;
    width?: string;
}

// eslint-disable-next-line react/display-name
export const SelectSchedule = forwardRef<HTMLDivElement, SelectScheduleProps>(
    ({ scheduleUserId, scheduleId, onChange, width, ...props }, ref) => {
        const schedules = useSelector(selectSchedules);

        const value = schedules.find((s) => s.userId === scheduleUserId && s.id === scheduleId);

        return (
            <Autocomplete
                sx={{ minWidth: '150px', width }}
                value={value ?? null}
                options={schedules}
                getOptionLabel={(s: Schedule) => s.title}
                renderOption={(props, option, state) => (
                    <li {...props}>
                        {option.isRecommended && <span style={{ color: green[500] }}>(рек.) </span>}
                        {option.title}
                    </li>
                )}
                isOptionEqualToValue={(o, v) => o.id === v.id && o.userId === v.userId}
                renderInput={(params) => <FormField {...params} label="" withoutErrorMessage />}
                onChange={(event, newValue) => onChange(newValue ?? undefined)}
            />
        );
    }
);

interface SelectScheduleFormProps extends FormFieldProps {
    registeredName: string;
}

// eslint-disable-next-line react/display-name
export const SelectScheduleForm = forwardRef<HTMLDivElement, SelectScheduleFormProps>(
    ({ registeredName, ...props }, ref) => {
        const schedules = useSelector(selectSchedules);

        return (
            <Controller
                name={registeredName}
                render={({ field: { value, ...field } }) => {
                    return (
                        <Autocomplete
                            value={value ?? null}
                            {...field}
                            options={schedules}
                            getOptionLabel={(s: Schedule) => s.title}
                            renderOption={(props, option, state) => (
                                <li {...props}>
                                    {option.isRecommended && <span style={{ color: green[500] }}>(рек.) </span>}
                                    {option.title}
                                </li>
                            )}
                            isOptionEqualToValue={(o, v) => o.id === v.id && o.userId === v.userId}
                            renderInput={(params) => <FormField {...params} {...props} />}
                            onChange={(event, newValue) => field.onChange(newValue ?? null)}
                        />
                    );
                }}
            />
        );
    }
);
