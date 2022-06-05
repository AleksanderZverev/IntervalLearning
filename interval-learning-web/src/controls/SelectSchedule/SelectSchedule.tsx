import { Autocomplete, Stack } from '@mui/material';
import { green } from '@mui/material/colors';
import { forwardRef, useMemo } from 'react';
import { Controller } from 'react-hook-form';
import { useSelector } from 'react-redux';
import { selectSchedules } from '../../redux/slices/scheduleSlice';
import { Schedule } from '../../types/schedule';
import { FormField, FormFieldProps } from '../Form/Form';

export interface ScheduleKey {
    scheduleUserId: string | undefined;
    scheduleId: string | undefined;
}

interface SelectScheduleProps {
    scheduleUserId: string | undefined;
    scheduleId: string | undefined;
    onChange: (newItem: Schedule | undefined) => void;
    width?: string;
    availableSchedules?: ScheduleKey[];
    defaultScheduleUserId?: string;
    defaultScheduleId?: string;
    showWordsPerPhase?: boolean;
}

// eslint-disable-next-line react/display-name
export const SelectSchedule = forwardRef<HTMLDivElement, SelectScheduleProps>(
    ({ scheduleUserId, scheduleId, onChange, width, availableSchedules, showWordsPerPhase, ...props }, ref) => {
        const schedules = useSelector(selectSchedules);

        const options = useMemo(() => {
            if (!availableSchedules) {
                return [...schedules];
            }

            return schedules.filter((s) =>
                availableSchedules.find((a) => a.scheduleUserId === s.userId && a.scheduleId === s.id)
            );
        }, [schedules, availableSchedules]);

        const value =
            scheduleUserId !== undefined && scheduleId !== undefined
                ? schedules.find((s) => s.userId === scheduleUserId && s.id === scheduleId)
                : props.defaultScheduleUserId !== undefined && props.defaultScheduleId !== undefined
                ? schedules.find((s) => s.userId === props.defaultScheduleUserId && s.id === props.defaultScheduleId)
                : undefined;

        return (
            <Stack gap={'20px'} direction="row" alignItems={'baseline'}>
                <Autocomplete
                    sx={{ minWidth: '150px', width }}
                    value={value ?? null}
                    options={options}
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
                {showWordsPerPhase && value && (
                    <div style={{ marginTop: 2 }}>Слов в этапе: {value.cardsCountPerPhase}</div>
                )}
            </Stack>
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
