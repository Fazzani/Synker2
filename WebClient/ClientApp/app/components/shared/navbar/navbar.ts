import { Component, NgModule } from '@angular/core';
import { MdButtonModule, MdMenuModule, MdIconModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import './navbar.scss';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.html'
})
export class NavBar {
    color: string = "primary";
}

@NgModule({
    imports: [MdButtonModule, MdMenuModule, MdIconModule, RouterModule],
    exports: [NavBar],
    declarations: [NavBar],
})
export class NavBarModule { }