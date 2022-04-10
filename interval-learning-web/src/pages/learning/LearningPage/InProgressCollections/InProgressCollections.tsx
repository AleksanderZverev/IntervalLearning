import React, { FC } from 'react';
import { withQueryResolver } from '../../../../hoc/withQueryResolver';
import useTypedSelector from '../../../../hooks/useTypedSelector';
import { useGetLearningCollectionsQuery } from '../../../../redux/api/learningApi';
import { selectQueueCards } from '../../../../redux/slices/queueLearnSlice';

export const InProgressCollectionsContent: FC = () => {
    const queueCards = useTypedSelector(selectQueueCards);
    return (
        <div>
            {queueCards.map((c) => (
                <div key={c.repeatDate.toLocaleString() + c.card.id.toString()}>
                    {c.card.frontSideText}-{c.card.backSideText}-{c.repeatDate.toLocaleDateString()}
                </div>
            ))}
        </div>
    );
};

export const InProgressCollections = withQueryResolver(useGetLearningCollectionsQuery)(InProgressCollectionsContent);
