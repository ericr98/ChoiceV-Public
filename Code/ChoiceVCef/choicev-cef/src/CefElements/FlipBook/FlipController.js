import React from 'react';
import { register } from './../../App';
import HTMLFlipBook from "react-pageflip";
import { Document, Page as ReactPdfPage } from "react-pdf";
import 'react-pdf/dist/Page/TextLayer.css';
import 'react-pdf/dist/Page/AnnotationLayer.css';

import { url } from './../../index';

import "./style.css";

import { pdfjs } from 'react-pdf';

pdfjs.GlobalWorkerOptions.workerSrc = `http://choicev-cef.net/src/cef/flipbooks/pdf.worker.min.mjs`;

if (typeof Promise.withResolvers === 'undefined') {
    if (window)
        // @ts-expect-error This does not exist outside of polyfill which this is doing
        window.Promise.withResolvers = function () {
            let resolve, reject;
            const promise = new Promise((res, rej) => {
                resolve = res;
                reject = rej;
            });
            return { promise, resolve, reject };
        };
}


const Page = React.forwardRef(({ pageNumber, width }, ref) => {
    return (
        <div ref={ref}>
            <ReactPdfPage pageNumber={pageNumber} width={width} />
        </div>
    );
});

export default class FlipBookController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            pdf: null,
            savedPdfs: new Map(),
        };

        this.onOpenPdf = this.onOpenPdf.bind(this);
        this.onClosePdf = this.onClosePdf.bind(this);
        this.onOpenPdfUrl = this.onOpenPdfUrl.bind(this);

        this.props.input.registerEvent("OPEN_PDF", this.onOpenPdf);
        this.props.input.registerEvent("OPEN_PDF_URL", this.onOpenPdfUrl);
        this.props.input.registerEvent("CLOSE_CEF", this.onClosePdf);
    }
    
    onOpenPdf(data) {    
        if(this.state.savedPdfs.has(data.identifier)) {
            this.setState({
                pdf: this.state.savedPdfs.get(data.identifier),
            });
        } else {
            if(data.action === "answer") {
                this.setState({
                    pdf: `data:application/pdf;base64,${data.data}`
                });
                this.state.savedPdfs.set(data.identifier, `data:application/pdf;base64,${data.data}`);
            } else {
                this.props.output.sendToServer("REQUEST_PDF_FILE", {identifier: data.identifier}, false);
            }
        }
    }

    onOpenPdfUrl(data) {
        console.log(`${url}flipbooks/${data.url}`);
        this.setState({
            pdf: `${url}flipbooks/${data.url}`
        });
    }

    onClosePdf() {
        this.setState({
            pdf: null
        });
    }

    render() {
        if (this.state.pdf != null) {
            return (
                <FlipBookComponent pdf={this.state.pdf} />
            )
        } else {
            return null;
        }
    }
}

class FlipBookComponent extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            pdf: this.props.pdf,
            page: 0,
            totalPages: 0,
            pages: []
        };


        this.onDocumentLoadSuccess = this.onDocumentLoadSuccess.bind(this);
        this.nextButtonClick = this.nextButtonClick.bind(this);
        this.prevButtonClick = this.prevButtonClick.bind(this);
        this.onPage = this.onPage.bind(this);
        this.handleKeyDown = this.handleKeyDown.bind(this);

        this.flipBook = React.createRef();
    }

    componentDidMount() {
        this.setState({height: window.innerHeight, width: window.innerWidth});
    }

    nextButtonClick() {
        //if(this.flipBook.getPageFlip) {
            this.flipBook.pageFlip().flipNext();
        //}
    };

    prevButtonClick() {
        //if(this.flipBook.pageFlip) {
            this.flipBook.pageFlip().flipPrev();
        //}
    };

    onPage(e) {
        if(this.state.pdf != null) {
            var val = e.data;
            this.setState({
                page: val,
            }, () => {
                this.input.value = val;
            });        
        }
    };

    onDocumentLoadSuccess({ numPages }) {
        var l = [];
        for(var i = 1; i<=numPages; i++) {
            l.push(i);
        }
        this.setState({totalPages: numPages, pages: l});
    }

    handleKeyDown(e) {
        if (e.key === 'Enter') {
            if(e.target.value != "" && !isNaN(e.target.value)) {
                this.flipBook.pageFlip().flip(parseInt(e.target.value));
            }
        }
    }

    render() {
        if (this.state.pdf != null) {
            const width = Math.round(this.state.width * 0.36);
            const height = Math.round(this.state.height * 0.905);
            return (
                <div id="flipBookWrapper">
                    <div></div>
                    <div className="standardWrapper noSelect">
                        <div style={{ width: (width * 2) + "px", height: "100%", overflow: "hidden" }}>
                            <Document
                            file={this.state.pdf}
                            onLoadSuccess={this.onDocumentLoadSuccess}>
                                {this.state.totalPages > 1 ?
                                <HTMLFlipBook
                                    width={width}
                                    height={height}
                                    ref={(el) => (this.flipBook = el)}
                                    onFlip={this.onPage}
                                    flippingTime={500}
                                    autoSize={true}
                                    // drawShadow={false}
                                    disableFlipByClick={true}
                                    showPageCorners={false}
                                    renderOnlyPageLengthChange={true} >
                                    {this.state.pages.map((el) => {
                                        return (<Page pageNumber={el} width={width} height={height} />);
                                    })}
                                </HTMLFlipBook>
                                : <div className="standardWrapper" style={{paddingTop: "2.5%"}}><Page pageNumber={1} width={750} /></div>
                                }
                            </Document>
                        </div>
                    </div>
                    {this.state.totalPages > 1 ?
                        <div className="standardWrapper">
                            <div id="flipBookContainer">
                                <button type="button" className="flipBookButton" onClick={this.prevButtonClick}>
                                    Vorherige Seite
                                </button>

                                <div className="flipBookPages" style={{zIndex: 120}}>
                                    <input type="number" ref={(el) => (this.input = el)} defaultValue={this.state.page} onKeyDown={this.handleKeyDown}></input> von
                                    <span> {this.state.totalPages}</span>
                                </div>

                                <button type="button" className="flipBookButton" onClick={this.nextButtonClick}>
                                    Nächste Seite
                                </button>
                            </div>
                        </div>
                    : null}   
                </div>
            );
        } else {
            return null;
        }
    }
}

// export default class FlipBookController extends React.Component {
//     constructor(props) {
//         super(props);

//         this.state = {
//             totalPages: 0,
//             pages: [],
//             page: 0,
//         };

//         this.onDocumentLoadSuccess = this.onDocumentLoadSuccess.bind(this);
//         this.nextButtonClick = this.nextButtonClick.bind(this);
//         this.prevButtonClick = this.prevButtonClick.bind(this);
//         //this.onSelectPage = this.onSelectPage.bind(this);
//         this.onPage = this.onPage.bind(this);
//     }


//     componentWillMount() {
//         this.setState({height: window.innerHeight, width: window.innerWidth});
//     }

//     nextButtonClick() {
//         console.log(this.flipBook);
//         this.flipBook.getPageFlip().flipNext();
//     };

//     prevButtonClick() {
//         this.flipBook.getPageFlip().flipPrev();
//     };

//     onPage(e) {
//         this.setState({
//             page: e.data,
//         });
//     };
    
//     onSelectPage(e) {
//         return;
//         var input = e.target.value;
//         this.setState({
//             page: input,
//         }, () => {
//             if(!isNaN(input)) {
//                 this.flipBook.getPageFlip().flip(input);
//             }
//         });

//     };

//     onDocumentLoadSuccess({ numPages }) {
//         var l = [];
//         for(var i = 1; i<=numPages; i++) {
//             l.push(i);
//         }
//         this.setState({totalPages: numPages, pages: l});
//     }

//     render() {     
//         const width = this.state.width * 0.30;
//         const height = this.state.height * 0.85;
        
//         return (
//                 <div id="flipBookWrapper">
//                     <div></div>
//                     <div className="standardWrapper">
//                         <div style={{ width: width * 2 + "px", height: height }}>
//                             <Document file="/cef/flipbooks/test/test.pdf" onLoadSuccess={this.onDocumentLoadSuccess}>
//                                 <HTMLFlipBook 
//                                     width={width} 
//                                     height={height}
//                                     ref={(el) => (this.flipBook = el)}
//                                     onFlip={this.onPage} >
//                                         {this.state.pages.map((el) => {
//                                             return (<Page pageNumber={el} width={width} />);
//                                         })}
//                                 </HTMLFlipBook>
//                             </Document>
//                         </div>
//                     </div>
//                     <div className="standardWrapper">
//                         <div id="flipBookContainer">
//                             <button type="button" className="flipBookButton" onClick={this.prevButtonClick}>
//                                 Vorherige Seite
//                             </button>

//                             <div className="flipBookPages">
//                                 {/* <input type="number" value={this.state.page} ></input> von  */}
//                                 {/* <span> {this.state.totalPages}</span> */}
//                             </div>

//                             <button type="button" className="flipBookButton" onClick={this.nextButtonClick}>
//                                 Nächste Seite
//                             </button>
//                         </div>
//                     </div>
//                 </div>
//         );
//     }
// }



register(FlipBookController);