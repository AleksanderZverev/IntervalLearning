import { Card } from '../../types/Collection';

export class CardHelper {
    static GetCardUniqueId(card: Card): string {
        return `${card.userId}-${card.collectionId}-${card.userId}`;
    }
}
