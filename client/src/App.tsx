import * as React from 'react';
import './App.css';
import { fetchValues } from './services/values.service';

import { Collapse, DropdownItem, DropdownMenu, DropdownToggle, Nav, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink, UncontrolledDropdown,  } from 'reactstrap';

class App extends React.Component<any, any> {
  constructor(props: any) {
    super(props);

    this.toggle = this.toggle.bind(this);
    this.state = {
      isOpen: false
    };

    fetchValues().then(async resp => {
      // tslint:disable-next-line:no-console
      console.log(await resp.text());
    });
  }
  public toggle() {
    this.setState({
      isOpen: !this.state.isOpen
    });
  }
  public render() {
    return (
      <div className="App">
        <Navbar dark={true} expand="md" color="primary">
          <NavbarBrand href="/">Visual Studio Enterprise Extension Manager</NavbarBrand>
          <NavbarToggler onClick={this.toggle} />
          <Collapse isOpen={this.state.isOpen} navbar={true}>
            <Nav className="ml-auto" navbar={true}>
              <NavItem>
                <NavLink href="/components/">Components</NavLink>
              </NavItem>
              <NavItem>
                <NavLink href="https://github.com/reactstrap/reactstrap">GitHub</NavLink>
              </NavItem>
              <UncontrolledDropdown nav={true} inNavbar={true}>
                <DropdownToggle nav={true} caret={true}>
                  Options
                </DropdownToggle>
                <DropdownMenu right={true}>
                  <DropdownItem>
                    Option 1
                  </DropdownItem>
                  <DropdownItem>
                    Option 2
                  </DropdownItem>
                  <DropdownItem divider={true} />
                  <DropdownItem>
                    Reset
                  </DropdownItem>
                </DropdownMenu>
              </UncontrolledDropdown>
            </Nav>
          </Collapse>
        </Navbar>

      </div>
    );
  }
}

export default App;
