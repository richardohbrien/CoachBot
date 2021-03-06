import { Injectable } from '@angular/core';
import { Server } from '../../model/server';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ServerService {

    constructor(private http: HttpClient) { }

    getServers(): Observable<Server[]> {
        return this.http.get<Server[]>(`${environment.apiUrl}/api/server`).pipe();
    }

    getServer(id: number): Observable<Server> {
        return this.http.get<Server>(`${environment.apiUrl}/api/server/${id}`).pipe();
    }

    addServer(server: Server) {
        return this.http.post(`${environment.apiUrl}/api/server`, server).pipe();
    }

    removeServer(id: number) {
        return this.http.delete(`${environment.apiUrl}/api/server/${id}`).pipe();
    }
}
