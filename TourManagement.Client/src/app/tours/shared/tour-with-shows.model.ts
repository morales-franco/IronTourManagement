import { Show } from '../shows/shared/show.model';
import { Tour } from './tour.model';

export class TourWithShows extends Tour {
    shows: Show[]
}
