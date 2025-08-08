import React from 'react';
import './style/App.css';

var registeredElements = [];
var currentId = 0;
export function register(el) {
    registeredElements.push({id: currentId, El: el});
    currentId++;
}

export default class App extends React.Component {
  constructor(props) {
    super(props);
  }

  render() {
    return registeredElements.map((el) => {
      return <el.El key={el.id} input={this.props.input} output={this.props.output} auth={this.props.auth} />
    });
  }
}