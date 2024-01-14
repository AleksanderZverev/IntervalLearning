import { Card } from '../../../types/Collection';
import _ from 'lodash';

export class PagedCards {
    private pageToCardIds: { [page: number]: Card[] } = {};

    private copy = (): PagedCards => {
        const result = new PagedCards();
        result.pageToCardIds = this.pageToCardIds;
        return result;
    };

    getCardsForPage = (page: number): Card[] => {
        return page in this.pageToCardIds ? this.pageToCardIds[page] : [];
    };

    setCardsForPage = (page: number, cards: Card[]): PagedCards => {
        this.pageToCardIds[page] = [...cards];
        return this.copy();
    };

    addCardToFirstPage = (card: Card): PagedCards => {
        if (!(1 in this.pageToCardIds)) {
            this.pageToCardIds[1] = [card];
            return this.copy();
        }
        const oldCards = this.pageToCardIds[1];
        this.pageToCardIds[1] = [card, ..._.take(oldCards, oldCards.length - 1)];
        return this.copy();
    };

    deleteCardFromPages = (card: Card): PagedCards => {
        const pages = Object.keys(this.pageToCardIds);
        for (const page in pages) {
            const cards = this.pageToCardIds[page];
            if (_.some(cards, (c) => c.id === card.id)) {
                this.pageToCardIds[page] = _.remove(cards, (card) => card.id === card.id);
            }
        }
        return this.copy();
    };
}
