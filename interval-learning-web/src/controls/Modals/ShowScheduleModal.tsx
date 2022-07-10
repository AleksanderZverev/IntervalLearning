import { Loop, Title } from '@mui/icons-material';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
    Stack,
    Tooltip,
    Typography,
} from '@mui/material';
import dayjs from 'dayjs';
import { FC, useMemo } from 'react';
import { getIntervals } from '../../pages/schedules/ScheduleListPage/ScheduleListPage';
import { getForgottenBehaviorTitle, PhaseInfo, Schedule } from '../../types/schedule';
import { Label } from '../Label/Label';

interface ShowScheduleModalProps {
    schedule: Schedule;
    open: boolean;
    onClose: () => void;
}

interface PhaseItem extends PhaseInfo {
    hasRepeatPhase: boolean;
}

export const ShowScheduleModal: FC<ShowScheduleModalProps> = ({ schedule, open, onClose }) => {
    const sortedPhases = useMemo(
        () => [...schedule.phases].sort((f, s) => parseInt(f.id) - parseInt(s.id)),
        [schedule]
    );

    const phaseItems = useMemo(() => {
        const items = [];

        for (let i = 0; i < sortedPhases.length; i++) {
            const phase = sortedPhases[i];

            let hasRepeatPhase = false;

            if (i + 1 < sortedPhases.length) {
                const nextPhase = sortedPhases[i + 1];
                hasRepeatPhase = nextPhase.secondsFromLastPhase <= 10;
            }

            const item: PhaseItem = {
                ...phase,
                hasRepeatPhase,
            };

            items.push(item);

            if (hasRepeatPhase) {
                i++;
            }
        }

        return items;
    }, [sortedPhases]);

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>{schedule.title}</DialogTitle>
            <DialogContent>
                <Stack rowGap={'15px'}>
                    <Label label="Интервалы">
                        <div>{getIntervals(schedule)}</div>
                    </Label>
                    <Label label="При забывании">
                        <div>{getForgottenBehaviorTitle(schedule.forgottenBehavior)}</div>
                    </Label>
                    <Label label="При старте: короткое описание">
                        <div>{schedule.shortDescription}</div>
                    </Label>
                    <Label label="При старте: описание">
                        <div>{schedule.description}</div>
                    </Label>
                    <Label label="Для всех этапов: короткое описание">
                        <div>{schedule.defaultPhaseShortDescription}</div>
                    </Label>
                    <Label label="Для всех этапов: описание">
                        <div>{schedule.defaultPhaseDescription}</div>
                    </Label>
                    <Label label="Для всех этапов повторения: короткое описание">
                        <div>{schedule.defaultRepeatPhaseShortDescription}</div>
                    </Label>
                    <Label label="Для всех этапов повторения: описание">
                        <div>{schedule.defaultRepeatPhaseDescription}</div>
                    </Label>
                    <Stack rowGap={'20px'}>
                        <Typography variant="h5">Интервалы</Typography>
                        {phaseItems.map((p, i, a) => {
                            const duration = dayjs.duration(p.secondsFromLastPhase, 's');

                            return (
                                <>
                                    <div key={p.id}>
                                        <Stack direction={'row'} alignItems="center" columnGap={'5px'}>
                                            <Divider orientation="vertical" />
                                            <Typography variant="h6">
                                                {i == 0 && duration.asSeconds() < 10
                                                    ? 'Повторение после старта'
                                                    : duration.humanize()}
                                            </Typography>
                                            {p.hasRepeatPhase && (
                                                <Tooltip title={'Повторять после изучения'} placement="top-start">
                                                    <Loop color="primary" />
                                                </Tooltip>
                                            )}
                                            {p.isDefaultValueSide && (
                                                <Tooltip
                                                    title={'Показывать обратную сторону (перевод) карточки'}
                                                    placement="top-start"
                                                >
                                                    <Title color="primary" />
                                                </Tooltip>
                                            )}
                                        </Stack>
                                        <Stack key={p.id} rowGap="15px" sx={{ margin: '10px 0 0 6px' }}>
                                            {p.shortDescription && (
                                                <Label label="Переопределенное короткое описание">
                                                    <div>{p.shortDescription}</div>
                                                </Label>
                                            )}
                                            {p.description && (
                                                <Label label="Переопределенное описание">
                                                    <div>{p.description}</div>
                                                </Label>
                                            )}
                                        </Stack>
                                    </div>
                                    {i + 1 < a.length && <Divider key={p.id + 'divider'} />}
                                </>
                            );
                        })}
                    </Stack>
                </Stack>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Закрыть</Button>
            </DialogActions>
        </Dialog>
    );
};
