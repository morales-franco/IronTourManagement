import { HttpInterceptor, HttpEvent, HttpRequest, HttpHandler } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';

export class EnsureAcceptHeaderInterceptor implements HttpInterceptor {

    //@FM: Create interceptor to ensure that all request have an Accept Media type by default
    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        if(!request.headers.has("accept")){
            request  = request.clone({ headers : request.headers.set('Accept', 'application/json') });
        }

        return next.handle(request);
    }

}
