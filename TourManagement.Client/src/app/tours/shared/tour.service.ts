import { Injectable, ErrorHandler } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/throw';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/do';

import { Tour } from './tour.model';
import { BaseService } from '../../shared/base.service';
import { TourWithEstimatedProfits } from './tour-with-estimated-profits.model';
import { TourWithManagerForCreation } from './tour-with-manager-for-creation.model';
import { TourForCreation } from './tour-for-creation.model';
import { TourWithEstimatedProfitsAndShows } from './tour-with-estimated-profits-and-shows.model';
import { TourWithShows } from './tour-with-shows.model';
import { TourWithShowsForCreation } from './tour-with-shows-for-creation.model';
import { TourWithManagerAndShowsForCreation } from './tour-with-manager-and-shows-for-creation.model';

@Injectable()
export class TourService extends BaseService {

    constructor(private http: HttpClient) {           
        super();      
    }

    getTours(): Observable<Tour[]> {
        return this.http.get<Tour[]>(`${this.apiUrl}/tours`);
    }

    getTour(tourId: string): Observable<Tour> {
        return this.http.get<Tour>(`${this.apiUrl}/tours/${tourId}`);
    }

    getTourWithEstimatedProfits(tourId: string): Observable<TourWithEstimatedProfits> {
        return this.http.get<TourWithEstimatedProfits>(`${this.apiUrl}/tours/${tourId}`,
        { headers: { 'accept': 'application/vnd.iron.tourwithestimatedprofits+json'  } });
    }

    getTourWithShows(tourId: string): Observable<TourWithShows> {
        return this.http.get<TourWithShows>(`${this.apiUrl}/tours/${tourId}`,
        { headers: { 'accept': 'application/vnd.iron.tourwithshows+json'  } });
    }

    getTourWithEstimatedProfitsAndShows(tourId: string): Observable<TourWithEstimatedProfitsAndShows> {
        return this.http.get<TourWithEstimatedProfitsAndShows>(`${this.apiUrl}/tours/${tourId}`,
        { headers: { 'accept': 'application/vnd.iron.tourwithestimatedprofitsandshows+json'  } });
    }

    addTour(tourAdd: TourForCreation): Observable<Tour>{
        return this.http.post<Tour>(`${this.apiUrl}/tours`, tourAdd,
        { headers: { 'Content-Type': 'application/json'  } });
    }

    addTourWithManager(tourAdd: TourWithManagerForCreation): Observable<Tour>{
        return this.http.post<Tour>(`${this.apiUrl}/tours`, tourAdd,
        { headers: { 'Content-Type': 'application/vnd..tourwithmanagerforcreation+json'  } });
    }

    addTourWithShows(tourToAdd: TourWithShowsForCreation): Observable<Tour> {
        return this.http.post<Tour>(`${this.apiUrl}/tours`, tourToAdd,
            { headers: { 'Content-Type': 'application/vnd.iron.tourwithshowsforcreation+json' } });
    }
    
    addTourWithManagerAndShows(tourToAdd: TourWithManagerAndShowsForCreation): Observable<Tour> {
        return this.http.post<Tour>(`${this.apiUrl}/tours`, tourToAdd,
            { headers: { 'Content-Type': 'application/vnd.iron.tourwithmanagerandshowsforcreation+json' } });
    }
}
