import { Show } from '../shows/shared/show.model';
import { TourWithEstimatedProfits } from './tour-with-estimated-profits.model';

export class TourWithEstimatedProfitsAndShows extends TourWithEstimatedProfits {
    shows: Show[];
}
